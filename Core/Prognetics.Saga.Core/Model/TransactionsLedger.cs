namespace Prognetics.Saga.Core.Model;

public class TransactionsLedger
{
    public IReadOnlyList<Transaction> Transactions { get; init; } = new List<Transaction>();

    public TransactionStep? GetTransactionStepByCompletionEventName(string eventName)
        => Transactions
            .Select(x => x.GetStepByCompletionEventNameOrDefault(eventName))
            .FirstOrDefault(x => x != null);
}