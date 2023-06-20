namespace Prognetics.Saga.Orchestrator;
public record TransactionState
{
    public required string TransactionId { get; init; }
    public required string LastEvent { get; init; }
    public bool IsActive { get; init; } = true;
}