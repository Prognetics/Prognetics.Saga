using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Orchestrator.Contract;
using Prognetics.Saga.Orchestrator.Contract.DTO;

namespace Prognetics.Saga.Orchestrator;

public class SagaEngine : ISagaEngine
{
    private readonly ITransactionLedgerAccessor _transactionLedgerAccessor;

    public SagaEngine(ITransactionLedgerAccessor transactionLedgerAccessor)
    {
        _transactionLedgerAccessor = transactionLedgerAccessor;
    }

    public Task<EngineOutput?> Process(EngineInput input)
    {
        var transactionLedger = _transactionLedgerAccessor.TransactionsLedger;
        var transactionModel = transactionLedger.GetTransactionByCompletionEventName(input.EventName);
        var stepRecord = transactionModel?.GetStepByCompletionEventName(input.EventName);

        if (stepRecord is null || transactionModel is null)
        {
            return Task.FromResult((EngineOutput?)null);
        }

        return Task.FromResult((EngineOutput?)new EngineOutput(
            stepRecord.Step.CompletionEventName,
            new OutputMessage(
                input.Message.TransactionId ?? Guid.NewGuid().ToString(),
                input.Message.Payload)));
    }
}