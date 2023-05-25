using System.Collections.Immutable;
using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Orchestrator.Contract.DTO;

namespace Prognetics.Saga.Orchestrator;

public class SagaEngine : ISagaEngine
{
    private readonly ISagaLog _sagaLog;
    private readonly ITransactionLedgerProvider _transactionLedgerProvider;

    public SagaEngine(
        ISagaLog sagaLog,
        ITransactionLedgerProvider transactionLedgerProvider)
    {
        _sagaLog = sagaLog;
        _transactionLedgerProvider = transactionLedgerProvider;
    }

    public async Task<(string QueueName, OutputMessage Message)?> Process(
        string queueName,
        InputMessage inputMessage)
    {
        var transactionLedger = await _transactionLedgerProvider.Get();
        var operation = transactionLedger.Transactions.SelectMany(t => t.Steps).FirstOrDefault(s => 
            string.Equals(s.EventName, queueName, StringComparison.OrdinalIgnoreCase));
        if (operation is null){
            // TODO: Log error
            return null;
        }

        var transactionId = inputMessage.TransactionId;

        if (operation.Order == 0){
            transactionId = Guid.NewGuid().ToString(); //TODO: To service
            await _sagaLog.SetState(new TransactionState{
                TransactionId = transactionId,
                LastOperation = operation.EventName});
        }
        else{
            if (inputMessage.TransactionId is null)
            {
                // TODO: Log error
                return null;
            }

            var state = await _sagaLog.GetState(inputMessage.TransactionId);
            if (state is null){
                // TODO: Log error
                return null;
            }
            
            var transaction = transactionLedger.Transactions
                .First(t => t.Steps
                    .Any(s => s.EventName.Equals(state.LastOperation, StringComparison.OrdinalIgnoreCase)));
            
            var lastOperation = transaction.Steps.Single(s => s.EventName == state.LastOperation);
            var nextOperation = transaction.Steps.SingleOrDefault(s => s.Order == lastOperation.Order + 1);

            if (nextOperation == null
                || !string.Equals(
                    nextOperation.EventName,
                    operation.EventName,
                    StringComparison.OrdinalIgnoreCase))
            {
                // TODO: Log error
                return null;
            }

            await _sagaLog.SetState(new TransactionState{
                TransactionId = inputMessage.TransactionId,
                LastOperation = operation.EventName,
            });
        }

        if (inputMessage.Compensation is not null){
            _sagaLog.SaveCompensation(
                transactionId!,
                operation.CompensationEventName,
                inputMessage.Compensation);
        }

        return (
            operation.CompletionEventName,
            new OutputMessage(
                transactionId!,
                inputMessage.Payload));
    }

    public async Task<IReadOnlyDictionary<string, OutputMessage>> Compensate(string transactionId)
    {
        var transactionState = await _sagaLog.GetState(transactionId);
        if (transactionState is null){
            //TODO: Log warning/error
            return ImmutableDictionary<string, OutputMessage>.Empty;
        }
        await _sagaLog.SetState(new TransactionState{
            TransactionId = transactionId,
            LastOperation = transactionState.LastOperation,
            IsActive = false,
        });

        var compensations = await _sagaLog.GetCompensations(transactionId);
        return compensations.ToDictionary(
            c => c.Key,
            c => new OutputMessage(transactionId, c.Value));
    }
}
