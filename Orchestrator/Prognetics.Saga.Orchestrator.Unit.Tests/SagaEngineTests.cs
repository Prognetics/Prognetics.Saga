using Microsoft.Extensions.Logging;
using NSubstitute;
using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;
using Prognetics.Saga.Orchestrator.Contract.DTO;
using Prognetics.Saga.Orchestrator.Contract;

namespace Prognetics.Saga.Orchestrator.Unit.Tests;

public class SagaEngineTests
{
    private readonly SagaEngine _sagaEngine;
    private readonly ISagaLog _sagaLog;
    private readonly ICompensationStore _compensationStore;
    private readonly List<Transaction> _transactions;
    private readonly TransactionsLedger _transactionLedger;
    private readonly ITransactionLedgerAccessor _transactionLedgerAccessor;
    private readonly string _newId;
    private readonly ILogger<ISagaEngine> _logger;

    public SagaEngineTests()
    {
        _sagaLog = Substitute.For<ISagaLog>();
        _sagaLog.GetTransactionOrDefault(Arg.Any<string>()).Returns(Task.FromResult<TransactionLog?>(null));
        _compensationStore = Substitute.For<ICompensationStore>();
        _transactions = new()
        {
            new Transaction
            {
                Steps = new string[] { "First", "Middle", "Last" }.Select(x =>
                    new Step
                    {
                        CompletionEventName = $"{x}{nameof(Step.CompletionEventName)}",
                        NextEventName = $"{x}{nameof(Step.NextEventName)}",
                        CompensationEventName = $"{x}{nameof(Step.CompensationEventName)}",
                    }).ToList(),
            }
        };
        _transactionLedger = new (){ Transactions = _transactions};
        _transactionLedgerAccessor = Substitute.For<ITransactionLedgerAccessor>();
        _transactionLedgerAccessor.TransactionsLedger.Returns(_transactionLedger);
        _newId = Guid.NewGuid().ToString();
        _logger = Substitute.For<ILogger<ISagaEngine>>();

        _sagaEngine = new SagaEngine(
            _sagaLog,
            _compensationStore,
            _transactionLedgerAccessor,
            () => _newId,
            _logger
        );
    }

    [Fact]
    public async Task Process_UnknownEventName_ReturnsFailureAndLogsError()
    {
        // Arrange
        var input = new EngineInput { EventName = "UnknownEvent" };

        // Act
        var result = await _sagaEngine.Process(input);

        // Assert
        Assert.False(result.TryGetOutput(out _));
    }

    [Fact]
    public async Task Process_FirstStep_SuccessfullyInitializesTransaction()
    {
        // Arrange
        var input = new EngineInput
        {
            EventName = $"First{nameof(Step.CompletionEventName)}",
            Message =  new(null, new object(), null)
        };

        // Act
        var result = await _sagaEngine.Process(input);

        // Assert
        Assert.True(result.TryGetOutput(out var output));
        Assert.Equal($"First{nameof(Step.NextEventName)}", output.EventName);

        await _sagaLog.Received().AddTransaction(Arg.Any<TransactionLog>());
    }

    [Fact]
    public async Task Process_MissingTransactionId_ReturnsFailureAndLogsError()
    {
        // Arrange
        var input = new EngineInput { EventName = "MiddleCompletionEventName", Message = new(null, new(), null) };

        // Act
        var result = await _sagaEngine.Process(input);

        // Assert
        Assert.False(result.TryGetOutput(out _));
    }

    [Fact]
    public async Task Process_ExistingTransaction_ProcessesSuccessfully()
    {
        // Arrange
        var previousName = $"First{nameof(Step.CompletionEventName)}";
        var eventName = $"Middle{nameof(Step.CompletionEventName)}";
        var nextName = $"Middle{nameof(Step.NextEventName)}";
        var transactionId = Guid.NewGuid().ToString();
        var paylaod = new object();
        var compensation = "compensation";
        var input = new EngineInput
        {
            EventName = eventName,
            Message = new (transactionId, paylaod, compensation)
        };
        var transactionLog = new TransactionLog(
            transactionId,
            TransactionState.Active,
            previousName,
            DateTime.UtcNow);
        _sagaLog.GetTransactionOrDefault(transactionId)
            .Returns(Task.FromResult<TransactionLog?>(transactionLog));

        // Act
        var result = await _sagaEngine.Process(input);

        // Assert
        Assert.True(result.TryGetOutput(out var output));
        Assert.Equal(nextName, output.EventName);
        await _sagaLog.Received().UpdateTransaction(Arg.Any<TransactionLog>());
    }

    [Fact]
    public async Task Process_CompensationExists_SavesCompensation()
    {
        // Arrange
        var previousName = $"First{nameof(Step.CompletionEventName)}";
        var eventName = $"Middle{nameof(Step.CompletionEventName)}";
        var nextName = $"Middle{nameof(Step.NextEventName)}";
        var compensationName = $"Middle{nameof(Step.CompensationEventName)}";
        var transactionId = Guid.NewGuid().ToString();
        var payload = new object();
        var compensation = "compensation";
        var input = new EngineInput
        {
            EventName = eventName,
            Message = new(transactionId, payload, compensation)
        };
        var transactionLog = new TransactionLog(
            transactionId,
            TransactionState.Active,
            previousName,
            DateTime.UtcNow);
        _sagaLog.GetTransactionOrDefault(transactionId)
            .Returns(Task.FromResult<TransactionLog?>(transactionLog));

        // Act
        var result = await _sagaEngine.Process(input);

        // Assert
        Assert.True(result.TryGetOutput(out var output));
        Assert.Equal(nextName, output.EventName);
        await _sagaLog.Received().UpdateTransaction(Arg.Is<TransactionLog>(x => 
            x.TransactionId == transactionId
            && x.LastCompletionEvent == eventName
            && x.State == TransactionState.Active));
        await _compensationStore.Received().Save(new(new(transactionId, compensationName), compensation));
    }

    [Fact]
    public async Task Process_NoCompensation_DoesNotSaveCompensation()
    {
        // Arrange
        var previousName = $"First{nameof(Step.CompletionEventName)}";
        var eventName = $"Middle{nameof(Step.CompletionEventName)}";
        var nextName = $"Middle{nameof(Step.NextEventName)}";
        var compensationName = $"Middle{nameof(Step.CompensationEventName)}";
        var transactionId = Guid.NewGuid().ToString();
        var payload = new object();
        var input = new EngineInput
        {
            EventName = eventName,
            Message = new(transactionId, payload, null)
        };
        var transactionLog = new TransactionLog(
            transactionId,
            TransactionState.Active,
            previousName,
            DateTime.UtcNow);
        _sagaLog.GetTransactionOrDefault(transactionId)
            .Returns(Task.FromResult<TransactionLog?>(transactionLog));

        // Act
        var result = await _sagaEngine.Process(input);

        // Assert
        Assert.True(result.TryGetOutput(out var output));
        Assert.Equal(nextName, output.EventName);
        await _sagaLog.Received().UpdateTransaction(Arg.Is<TransactionLog>(x =>
            x.TransactionId == transactionId
            && x.LastCompletionEvent == eventName
            && x.State == TransactionState.Active));
        await _compensationStore.DidNotReceive().Save(Arg.Any<CompensationRow>());
    }
}