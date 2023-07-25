namespace Prognetics.Saga.Core.Model;

public class TransactionsLedger
{
    public IReadOnlyList<Transaction> Transactions { get; init; } = new List<Transaction>();

    public Transaction? GetTransactionByCompletionEventName(string eventName)
        => Transactions.FirstOrDefault(x => x.Steps.Any(s => s.CompletionEventName.ToLower() == eventName.ToLower()));
}