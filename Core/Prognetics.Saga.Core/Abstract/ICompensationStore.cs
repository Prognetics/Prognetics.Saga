using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Core.Abstract;

public interface ICompensationStore
{
    Task Save(
        CompensationRow compensationRow,
        CancellationToken cancellation = default);

    Task<IReadOnlyList<CompensationRow>> Get(
        string transactionId,
        CancellationToken cancellation = default);
}
