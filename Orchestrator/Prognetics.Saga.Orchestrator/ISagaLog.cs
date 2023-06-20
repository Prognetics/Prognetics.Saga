using System.Collections.Concurrent;

namespace Prognetics.Saga.Orchestrator;
public interface ISagaLog
{
    Task<TransactionState?> GetState(string transactionId);
    Task SetState(TransactionState transactionState);
}

public class SagaLog : ISagaLog
{
    private readonly ConcurrentDictionary<string, TransactionState> _transactions = new();
    public Task<TransactionState?> GetState(string transactionId)
        => Task.FromResult(_transactions.GetValueOrDefault(transactionId));

    public Task SetState(TransactionState transactionState)
    {
        _transactions[transactionState.TransactionId] = transactionState;
        return Task.CompletedTask;
    }
}