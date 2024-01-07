using Microsoft.Extensions.Logging;
using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;
using Prognetics.Saga.Orchestrator.Contract;
using Prognetics.Saga.Orchestrator.Contract.DTO;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Prognetics.Saga.Orchestrator;

public class SagaEngine : ISagaEngine
{
    private readonly ISagaLog _sagaLog;
    private readonly ICompensationStore _compensationStore;
    private readonly ITransactionLedgerAccessor _transactionLedgerAccessor;
    private readonly IdGenerator _idGenerator;
    private readonly ILogger<ISagaEngine> _logger;
    private readonly SemaphoreSlim _semaphore = new(1);

    public SagaEngine(
        ISagaLog sagaLog,
        ICompensationStore compensationStore,
        ITransactionLedgerAccessor transactionLedgerAccessor,
        IdGenerator idGenerator,
        ILogger<ISagaEngine> logger)
    {
        _sagaLog = sagaLog;
        _compensationStore = compensationStore;
        _transactionLedgerAccessor = transactionLedgerAccessor;
        _idGenerator = idGenerator;
        _logger = logger;
    }

    public async Task<EngineResult<EngineOutput>> Process(
        EngineInput input,
        CancellationToken cancellationToken = default)
    {
        var transactionLedger = _transactionLedgerAccessor.TransactionsLedger;
        var transactionStep = transactionLedger.GetTransactionStepByCompletionEventName(input.EventName);

        if (transactionStep is null)
        {
            _logger.LogError("Unknown event name: {EventName}", input.EventName);
            return EngineResult<EngineOutput>.Fail();
        }

        var transactionId = input.Message.TransactionId;

        if (transactionStep.IsFirst)
        {
            transactionId = await InitializeTransaction(transactionStep);
        }
        else if (transactionId is null)
        {
            _logger.LogError("Transaction id is missing");
            return EngineResult<EngineOutput>.Fail();
        }
        else if (await TryProcessExistingTransaction(transactionId, transactionStep) == false)
        {
            return EngineResult<EngineOutput>.Fail();
        }

        if (input.Message.Compensation is not null)
        {
            await _compensationStore.Save(
                new(
                    new(
                        transactionId,
                        transactionStep.Step.CompensationEventName),
                    JsonSerializer.Serialize(input.Message.Compensation)),
                cancellationToken);
        }

        return EngineResult<EngineOutput>.Success(
            new EngineOutput(
                transactionStep.Step.NextEventName,
                new OutputMessage(
                    transactionId,
                    input.Message.Payload)));
    }

    private async Task<string> InitializeTransaction(TransactionStep stepRecord)
    {
        var transactionId = _idGenerator();
        await _sagaLog.AddTransaction(
            new(
                transactionId,
                TransactionState.Active,
                stepRecord.Step.CompletionEventName,
                DateTime.UtcNow));
        return transactionId;
    }

    private async Task<bool> TryProcessExistingTransaction(string transactionId, TransactionStep transactionStep)
    {
        var transactionLog = await _sagaLog.GetTransactionOrDefault(transactionId);
        if (transactionLog is null)
        {
            _logger.LogError("Transaction with id: {TransactionId} has not been found", transactionId);
            return false;
        }

        var lastOperation = transactionStep.Transaction.GetStepByCompletionEventNameOrDefault(transactionLog.LastCompletionEvent);
        var nextOperation = lastOperation is not null
            ? transactionStep.Transaction.GetStepByOrderNumber(lastOperation.Order + 1)
            : null;

        if (transactionStep.Step != nextOperation)
        {
            _logger.LogError(
                "Transaction with id {TransactionId} expects {ExpectedEvent}, but received {ReceivedEvent}",
                transactionId,
                nextOperation?.CompletionEventName,
                transactionStep.Step.CompletionEventName);
            return false;
        }

        await _sagaLog.UpdateTransaction(new(
            transactionId,
            transactionStep.IsLast
                ? TransactionState.Finished
                : TransactionState.Active,
            transactionStep.Step.CompletionEventName,
            DateTime.UtcNow));

        return true;
    }

    public async Task<EngineResult<IEnumerable<EngineOutput>>> Compensate(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken);
            var transactionLog = await _sagaLog.GetTransactionOrDefault(transactionId, cancellationToken);

            if (CompensationCanBeExecuted(transactionId, transactionLog) == false)
            {
                return EngineResult<IEnumerable<EngineOutput>>.Fail();
            }

            await _sagaLog.UpdateTransaction(
                new(
                    transactionId,
                    TransactionState.Rollback,
                    transactionLog.LastCompletionEvent,
                    DateTime.UtcNow),
                cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }

        var compensations = await _compensationStore.Get(transactionId, cancellationToken);
        return EngineResult<IEnumerable<EngineOutput>>.Success(compensations.Select(c =>
            new EngineOutput(
                c.Key.CompensationEvent,
                new OutputMessage(transactionId, JsonSerializer.Deserialize<object>(c.Compensation) ?? new object()))));
    }

    public async Task CompleteRollback(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        var transactionLog = await _sagaLog.GetTransactionOrDefault(transactionId, cancellationToken);

        if (transactionLog == null)
        {
            _logger.LogWarning(
                "The transaction with id: {TransactionId} does not exist in the system, so rollback completion will not be executed.",
                transactionId);
            return;
        }

        if (transactionLog.State != TransactionState.Rollback)
        {
            _logger.LogWarning(
                "The transaction with id: {TransactionId} can not be completed its current state: {TransactionState}.",
                transactionId,
                transactionLog.State);
            return;
        }

        await _sagaLog.UpdateTransaction(
            new(
                transactionId,
                TransactionState.Failed,
                transactionLog.LastCompletionEvent,
                DateTime.UtcNow),
            cancellationToken);
    }

    private bool CompensationCanBeExecuted(
        string transactionId,
        [NotNullWhen(true)]TransactionLog? transactionLog)
    {
        if (transactionLog is null)
        {
            _logger.LogWarning(
                "The transaction with id: {TransactionId} does not exist in the system. Compensation will not be executed",
                transactionId);
            return false;
        }

        if (transactionLog.State == TransactionState.Failed)
        {
            _logger.LogWarning(
                "The transaction with id: {TransactionId} has already failed. Compensation will not be executed",
                transactionId);
            return false;
        }

        if (transactionLog.State == TransactionState.Rollback)
        {
            _logger.LogWarning(
                "The transaction with id: {TransactionId} is already in the process of rolling back. Compensation will not be executed.",
                transactionId);
            return false;
        }

        return true;
    }
}