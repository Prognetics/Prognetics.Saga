namespace Prognetics.Saga.Core.Model;

public class SagaTransactionStepModel
{
    public required int Order { get; set; }
    public required string From { get; init; }
    public required string To { get; init; }
    public required string Compensation { get; init; }
}