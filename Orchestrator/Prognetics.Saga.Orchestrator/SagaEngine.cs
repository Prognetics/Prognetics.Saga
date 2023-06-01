using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;
using Prognetics.Saga.Orchestrator.Contract.DTO;

namespace Prognetics.Saga.Orchestrator;

public class SagaEngine : ISagaEngine
{
    private readonly ISagaLog _sagaLog;
    private readonly ICompensationStore _compensationStore;
    private readonly ITransactionLedgerProvider _transactionLedgerProvider;
    private readonly IIdentifierService _identifierService;

    public SagaEngine(
        ISagaLog sagaLog,
        ICompensationStore compensationStore,
        ITransactionLedgerProvider transactionLedgerProvider,
        IIdentifierService identifierService)
    {
        _sagaLog = sagaLog;
        _compensationStore = compensationStore;
        _transactionLedgerProvider = transactionLedgerProvider;
        _identifierService = identifierService;
    }

    public async Task<EngineOutput?> Process(EngineInput input)
    {
        var transactionLedger = await _transactionLedgerProvider.Get();
        var transactionModel = transactionLedger.GetTransactionByEventName(input.EventName);
        var stepRecord = transactionModel?.GetStepByEventName(input.EventName);

        if (transactionModel is null || !stepRecord.HasValue){
            // TODO: Log error
            return null;
        }

        var transactionId = input.Message.TransactionId;

        if (stepRecord.Value.Order == 0)
        {
            transactionId = await CreateNewTransaction(stepRecord.Value.Step);
        }
        else
        {
            if (transactionId is null)
            {
                // TODO: Log error
                return null;
            }

            var state = await _sagaLog.GetState(transactionId);
            if (state is null){
                // TODO: Log error
                return null;
            }
            
            var lastOperation = transactionModel.GetStepByEventName(state.LastEvent);
            var nextOperation = lastOperation.HasValue
                ? transactionModel.GetStepByOrderNumber(lastOperation.Value.Order)
                : null;

            if (stepRecord.Value.Step != nextOperation)
            {
                // TODO: Log error
                return null;
            }

            await _sagaLog.SetState(new ()
            {
                TransactionId = transactionId,
                LastEvent = stepRecord.Value.Step.EventName,
            });
        }

        if (input.Message.Compensation is not null){
            await _compensationStore.SaveCompensation(new(
                transactionId,
                stepRecord.Value.Step.CompensationEventName,
                input.Message.Compensation));
        }

        return new (
            stepRecord.Value.Step.CompletionEventName,
            new OutputMessage(
                transactionId,
                input.Message.Payload));
    }

    private async Task<string> CreateNewTransaction(Step operationModel)
    {
        var transactionId = _identifierService.Generate();
        await _sagaLog.SetState(new TransactionState
        {
            TransactionId = transactionId,
            LastEvent = operationModel.EventName
        });

        return transactionId;
    }

    public async Task<IEnumerable<EngineOutput>> Compensate(string transactionId)
    {
        var transactionState = await _sagaLog.GetState(transactionId);
        if (transactionState is null)
        {
            //TODO: Log warning/error
            return Array.Empty<EngineOutput>();
        }
        await _sagaLog.SetState(new TransactionState{
            TransactionId = transactionId,
            LastEvent = transactionState.LastEvent,
            IsActive = false,
        });

        var compensations = await _compensationStore.GetCompensations(transactionId);
        return compensations.Select(c =>
            new EngineOutput(
                c.EventName,
                new OutputMessage(transactionId, c.Content)));
    }
}