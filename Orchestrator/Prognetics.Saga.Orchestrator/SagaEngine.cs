using Microsoft.Extensions.Logging;
using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;
using Prognetics.Saga.Orchestrator.Contract;
using Prognetics.Saga.Orchestrator.Contract.DTO;
using System.Diagnostics.CodeAnalysis;
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

    public async Task<EngineResult> Process(EngineInput input)
    {
        var transactionLedger = _transactionLedgerAccessor.TransactionsLedger;
        var transactionStep = transactionLedger.GetTransactionStepByCompletionEventName(input.EventName);

        if (transactionStep is null)
        {
            _logger.LogError("Unknown event name: {EventName}", input.EventName);
            return EngineResult.Fail();
        }

        var transactionId = input.Message.TransactionId;

        if (transactionStep.IsFirst)
        {
            transactionId = await InitializeTransaction(transactionStep);
        }
        else if (transactionId is null)
        {
            _logger.LogError("Transaction id is missing");
            return EngineResult.Fail();
        }
        else if (await TryProcessExistingTransaction(transactionId, transactionStep) == false)
        {
            return EngineResult.Fail();
        }

        if (CompensationExists(input.Message.Compensation))
        {
            await _compensationStore.Save(
                new(
                    transactionId,
                    transactionStep.Step.CompensationEventName,
                    input.Message.Compensation));
        }

        return EngineResult.Success(
            new EngineOutput(
                transactionStep.Step.NextEventName,
                new OutputMessage(
                    transactionId,
                    input.Message.Payload)));
    }

    private static bool CompensationExists([NotNullWhen(true)]string? compensation)
        => !string.IsNullOrEmpty(compensation);

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