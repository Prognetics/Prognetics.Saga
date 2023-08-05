namespace Prognetics.Saga.Orchestrator.Contract.DTO;

public record TransactionLog
{
    public long Id { get; }
    public required string TransactionId { get; init; }
    public required TransactionState State { get; init; }
    public string? CompletionEvent { get; init; }
}
