namespace Prognetics.Saga.Core.Model;

public class TransactionsLedger
{
    public IReadOnlyList<Transaction> Transactions { get; init; } = new List<Transaction>();
}