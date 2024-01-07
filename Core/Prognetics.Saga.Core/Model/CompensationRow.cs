namespace Prognetics.Saga.Core.Model;

public record CompensationRow(
    CompensationRowKey Key,
    string Compensation);

public record CompensationRowKey(
    string TransactionId,
    string CompensationEvent);