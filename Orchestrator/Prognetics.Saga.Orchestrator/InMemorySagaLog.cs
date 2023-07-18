using Prognetics.Saga.Orchestrator.Contract;
using Prognetics.Saga.Orchestrator.Contract.DTO;
using System.Collections.Concurrent;

namespace Prognetics.Saga.Orchestrator;

public class InMemorySagaLog : ISagaLog
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