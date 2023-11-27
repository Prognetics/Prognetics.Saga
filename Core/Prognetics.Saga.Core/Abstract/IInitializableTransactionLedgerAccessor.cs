namespace Prognetics.Saga.Core.Abstract;

public interface IInitializableTransactionLedgerAccessor : ITransactionLedgerAccessor
{
    Task Initialize(Action onUpdate, CancellationToken cancellation = default);
}