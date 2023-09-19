namespace Prognetics.Saga.Core.Model;

public record TransactionStep(int Order, Step Step, Transaction Transaction)
{
    public bool IsFirst => Order == 0;

    public bool IsLast => Transaction.Steps.Count == Order + 1;
}
