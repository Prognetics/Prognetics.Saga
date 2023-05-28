namespace Prognetics.Saga.Orchestrator;

public interface ICompensationStore{
    Task<IEnumerable<Compensation>> GetCompensations(string transactionId);
    Task SaveCompensation(Compensation compensation);
}