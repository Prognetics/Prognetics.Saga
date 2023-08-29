using Microsoft.Extensions.Logging;
using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;
using Prognetics.Saga.Orchestrator.Contract;
using Prognetics.Saga.Orchestrator.Contract.DTO;
using System.Transactions;

namespace Prognetics.Saga.Orchestrator;

public class SagaEngine : ISagaEngine
{
    private readonly ISagaLog _sagaLog;
    private readonly ICompensationStore _compensationStore;
    private readonly ITransactionLedgerAccessor _transactionLedgerAccessor;
    private readonly IdGenerator _idGenerator;
    private readonly ILogger<ISagaEngine> _logger;

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

    public async Task<EngineOutput?> ProcessOrDefault(EngineInput input)
    {
        var transactionLedger = _transactionLedgerAccessor.TransactionsLedger;
        var transactionStep = transactionLedger.GetTransactionStepByCompletionEventName(input.EventName);

        if (transactionStep is null)
        {
            _logger.LogError("Unknown event name: {EventName}", input.EventName);
            return null;
        }

        var transactionId = input.Message.TransactionId;

        if (transactionStep.IsFirst)
        {
            transactionId = await InitializeTransaction(transactionStep);
        }
        else if (transactionId is null)
        {
            _logger.LogError("Transaction id is missing");
            return null;
        }
        else if (await TryProcessExistingTransaction(transactionId, transactionStep) == false)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(input.Message.Compensation))
        {
            await _compensationStore.Save(
                new(
                    transactionId,
                    transactionStep.Step.CompensationEventName,
                    input.Message.Compensation));
        }

        return new(
            transactionStep.Step.CompletionEventName,
            new(
                transactionId,
                input.Message.Payload));
    }

    private async Task<string> InitializeTransaction(TransactionStep stepRecord)
    {
        var transactionId = _idGenerator();
        await _sagaLog.AddTransaction(
            new()
            {
                TransactionId = transactionId,
                State = TransactionState.Active,
                LastCompletionEvent = stepRecord.Step.CompletionEventName,
                LastUpdate = DateTime.UtcNow,
            });
        return transactionId;
    }

    private async Task<bool> TryProcessExistingTransaction(string transactionId, TransactionStep transactionStep)
    {
        var transactionLog = await _sagaLog.GetTransaction(transactionId);
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
                transactionStep.Step.CompletionEventName,
                nextOperation?.CompletionEventName);
            return false;
        }

        await _sagaLog.UpdateTransaction(new()
        {
            TransactionId = transactionId,
            State = transactionStep.IsLast
                ? TransactionState.Finished
                : TransactionState.Failed,
            LastCompletionEvent = transactionStep.Step.CompletionEventName,
            LastUpdate = DateTime.UtcNow,
        });

        return true;
    }
}