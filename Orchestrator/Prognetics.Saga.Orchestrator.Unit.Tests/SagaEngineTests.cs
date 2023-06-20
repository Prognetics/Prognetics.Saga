using NSubstitute;
using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;
using Prognetics.Saga.Orchestrator.Contract.DTO;

namespace Prognetics.Saga.Orchestrator.Unit.Tests;

public class SagaEngineTests
{
    private readonly ISagaLog _sagaLog = Substitute.For<ISagaLog>();
    private readonly ICompensationStore _compensationStore = Substitute.For<ICompensationStore>();
    private readonly ITransactionLedgerAccessor _transactionLedgerProvider = Substitute.For<ITransactionLedgerAccessor>();
    private readonly IIdentifierService _identifierService = Substitute.For<IIdentifierService>();
    private readonly SagaEngine _sut;
    private readonly TransactionsLedger _transactionLedger;
    private const int _transactionsCount = 5;
    private const int _stepsCountPerTransaction = 5;
    private const string _fromPrefix = nameof(_fromPrefix);
    private const string _toPrefix = nameof(_toPrefix);
    private const string _compensationPrefix = nameof(_compensationPrefix);
    private const string _stepNameTemplate = "{0}, transaction: {1}, step: {2}";

    public SagaEngineTests()
    {
        _sut = new SagaEngine(
            _sagaLog,
            _compensationStore,
            _transactionLedgerProvider,
            _identifierService);

        _transactionLedger = Enumerable.Range(0, _transactionsCount)
            .Aggregate(
                new ModelBuilder(),
                (builder, ti) =>
                 builder.AddTransaction(
                    $"transaction_{ti}",
                    transactionBuilder =>
                    {
                        foreach(var si in Enumerable.Range(0, _stepsCountPerTransaction))
                        {
                            transactionBuilder.AddStep(
                                string.Format(_stepNameTemplate, _fromPrefix, ti, si),
                                string.Format(_stepNameTemplate, _toPrefix, ti, si),
                                string.Format(_stepNameTemplate, _compensationPrefix, ti, si));
                        }
                    }))
                                    
            .Build();

        _transactionLedgerProvider.TransactionsLedger.Returns(_transactionLedger);
    }

    [Fact]
    public async Task IfProcessInputWithInvalidEventNameThenEngineShouldReturnNull()
    {
        // Act
        var result = await _sut.Process(new ("invalid event name", new (null, new (), null)));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task IfMessageIsNotFirstAndDoesntHaveTransactionIdThenEngineShouldReturnNull()
    {
        // Arrange
        var engineInput = new EngineInput(
            string.Format(
                _stepNameTemplate,
                _fromPrefix,
                Random.Shared.Next(_transactionsCount - 1),
                Random.Shared.Next(1, _stepsCountPerTransaction - 1)),
            new(null, new(), null));

        // Act
        var result = await _sut.Process(engineInput);

        // Assert
        Assert.Null(result);
        await _compensationStore
           .DidNotReceiveWithAnyArgs()
           .SaveCompensation(Arg.Any<Compensation>());
    }

    [Fact]
    public async Task IfMessageIsFirstInTransactionThenEngineShouldProcessItCorrectlyAndReturnCorrectOutputMessage()
    {
        // Arrange
        var transactionId = Guid.NewGuid().ToString();
        _identifierService.Generate().Returns(transactionId);

        var payload = new { Data = "Data" };
        var compensation = new { CompensationData = "CompensationData" };

        var inputMessage = new InputMessage(
            null,
            payload,
            compensation);

        var transactionIndex = Random.Shared.Next(_transactionsCount - 1);
        var stepIndex = 0;
        var eventName = string.Format(
            _stepNameTemplate,
            _fromPrefix,
            transactionIndex,
            stepIndex);

        var engineInput = new EngineInput(
            eventName,
            inputMessage);

        // Act
        var result = await _sut.Process(engineInput);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(result.Value, new(
            string.Format(_stepNameTemplate, _toPrefix, transactionIndex, stepIndex),
            new(transactionId, payload)));

        await _sagaLog.Received(1).SetState(
            Arg.Is<TransactionState>(
                ts => ts.Equals(new TransactionState
                {
                    TransactionId = transactionId,
                    LastEvent = eventName,
                })));

        await _compensationStore
            .Received(1)
            .SaveCompensation(Arg.Is<Compensation>(c => 
                c.Equals(new Compensation(
                    transactionId,
                    string.Format(
                        _stepNameTemplate,
                        _compensationPrefix,
                        transactionIndex,
                        stepIndex),
                    compensation))));
    }

    [Fact]
    public async Task IfMessageIsNotFirstTransationStateIsNotPresentThenEngineShouldReturnNull()
    {
        // Arrange
        var transactionId = Guid.NewGuid().ToString();
        var payload = new { Data = "Data" };
        var compensation = new { CompensationData = "CompensationData" };

        var inputMessage = new InputMessage(
            transactionId,
            payload,
            compensation);

        var engineInput = new EngineInput(
            string.Format(
                _stepNameTemplate,
                _fromPrefix,
                Random.Shared.Next(_transactionsCount - 1),
                Random.Shared.Next(1, _stepsCountPerTransaction - 1)),
            inputMessage);

        _sagaLog.GetState(transactionId)
            .Returns(Task.FromResult<TransactionState?>(null));

        // Act
        var output = await _sut.Process(engineInput);

        // Assert
        Assert.Null(output);
    }

    [Fact]
    public async Task IfMessageHasUnknownEventNameThenEngineShouldReturnNull()
    {
        // Arrange
        const string unknownEventName = nameof(unknownEventName);
        var transactionId = Guid.NewGuid().ToString();
        var payload = new { Data = "Data" };
        var compensation = new { CompensationData = "CompensationData" };

        var inputMessage = new InputMessage(
            transactionId,
            payload,
            compensation);

        var engineInput = new EngineInput(
            unknownEventName,
            inputMessage);

        _sagaLog.GetState(transactionId)
            .Returns(Task.FromResult<TransactionState?>(null));

        // Act
        var output = await _sut.Process(engineInput);

        // Assert
        Assert.Null(output);
    }

    [Fact]
    public async Task IfMessageIsNotFirstInTransactionThenEngineShouldProcessItCorrectlyAndReturnCorrectOuputMessage()
    {
        // Arrange
        var transactionId = Guid.NewGuid().ToString();
        var payload = new { Data = "Data" };
        var compensation = new { CompensationData = "CompensationData" };

        var inputMessage = new InputMessage(
            transactionId,
            payload,
            compensation);

        var transactionIndex = Random.Shared.Next(_transactionsCount - 1);
        var stepIndex = Random.Shared.Next(1, _stepsCountPerTransaction - 1);

        var lastEventName = string.Format(
            _stepNameTemplate,
            _fromPrefix,
            transactionIndex,
            stepIndex - 1);

        var eventName = string.Format(
            _stepNameTemplate,
            _fromPrefix,
            transactionIndex,
            stepIndex);

        var engineInput = new EngineInput(
            eventName,
            inputMessage);

        _sagaLog.GetState(transactionId).Returns(Task.FromResult<TransactionState?>(
            new TransactionState
            {
                TransactionId = transactionId,
                LastEvent = lastEventName
            }));

        // Act
        var result = await _sut.Process(engineInput);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(result.Value, new(
            string.Format(_stepNameTemplate, _toPrefix, transactionIndex, stepIndex),
            new(transactionId, payload)));

        _identifierService.DidNotReceive().Generate();

        await _sagaLog.Received(1).SetState(
            Arg.Is<TransactionState>(
                ts => ts.Equals(new TransactionState
                {
                    TransactionId = transactionId,
                    LastEvent = eventName,
                })));

        var outputEventName = string.Format(
            _stepNameTemplate,
            _compensationPrefix,
            transactionIndex,
            stepIndex);

        await _compensationStore
            .Received(1)
            .SaveCompensation(Arg.Is<Compensation>(c =>
                c.Equals(new Compensation(
                    transactionId,
                    outputEventName,
                    compensation))));
    }
}
