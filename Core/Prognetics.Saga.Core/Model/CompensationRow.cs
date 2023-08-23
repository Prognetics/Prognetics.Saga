namespace Prognetics.Saga.Core.Model;

public record CompensationRow(
    string TransactionId,
    string CompensationEvent,
    string Compensation);