namespace Prognetics.Saga.Core.Model;

public class TransactionsLedger
{
    private IReadOnlyDictionary<string, Transaction>? _eventToTransaction;

    public IReadOnlyList<Transaction> Transactions { get; init; } = new List<Transaction>();

    public Transaction? GetTransactionByEventName(string eventName)
    {
        if (_eventToTransaction is null)
        {
            _eventToTransaction = Transactions.SelectMany(
                t => t.Steps.ToDictionary(
                    s => s.EventName.ToUpper(),
                    _ => t))
                .ToDictionary(
                    x => x.Key,
                    x => x.Value);
        }
        return _eventToTransaction.GetValueOrDefault(eventName.ToUpper());
    }
}