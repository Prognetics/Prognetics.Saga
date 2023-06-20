namespace Prognetics.Saga.Core.Abstract;

public interface IInitializableTransactionLedgerAccessor : ITransactionLedgerAccessor
{
    Task Initialize(CancellationToken cancellation = default);
}