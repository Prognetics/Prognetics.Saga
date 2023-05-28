namespace Prognetics.Saga.Core.Model;

public class SagaModel
{
    private readonly IReadOnlyDictionary<string, SagaTransactionModel> _eventToTransaction;

    public SagaModel(IReadOnlyList<SagaTransactionModel> transactions)
    {
        Transactions = transactions;
        _eventToTransaction = transactions.SelectMany(
            t => t.Steps.ToDictionary(
                s => s.EventName.ToUpper(),
                _ => t))
            .ToDictionary(
                x => x.Key,
                x => x.Value);
    }

    public IReadOnlyList<SagaTransactionModel> Transactions { get; }

    public SagaTransactionModel? GetSagaTransactionByEventName(string eventName)
        => _eventToTransaction.GetValueOrDefault(eventName.ToUpper());
}