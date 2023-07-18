using Prognetics.Saga.Orchestrator.Contract;
using Prognetics.Saga.Orchestrator.Contract.DTO;
using System.Collections.Concurrent;

namespace Prognetics.Saga.Orchestrator;

public class InMemoryCompensationStore : ICompensationStore
{
    private readonly ConcurrentDictionary<string, ConcurrentStack<Compensation>> _compensations  = new();
    public Task<IEnumerable<Compensation>> GetCompensations(string transactionId)
        => Task.FromResult(
            _compensations.GetValueOrDefault(transactionId)
                ?? (IEnumerable<Compensation>) Array.Empty<Compensation>());

    public Task SaveCompensation(Compensation compensation)
    {
        var transactionCompensations = _compensations.GetOrAdd(
            compensation.TransactionId,
            _ => new ConcurrentStack<Compensation>());

        transactionCompensations.Push(compensation);
        return Task.CompletedTask;
    }
}