using Prognetics.Saga.Orchestrator.Contract.DTO;

namespace Prognetics.Saga.Orchestrator.Contract;

public interface ICompensationStore
{
    Task<IEnumerable<Compensation>> GetCompensations(string transactionId);
    Task SaveCompensation(Compensation compensation);
}