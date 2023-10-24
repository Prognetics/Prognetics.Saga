using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Core.Abstract;

public interface IModelSource
{
    Task<TransactionsLedger> GetModel(CancellationToken cancellation = default);

    event EventHandler ModelChanged;
}
