namespace Prognetics.Saga.Core.Model;

public record TransactionLog
{
    public required string TransactionId { get; init; }
    public required TransactionState State { get; init; }
    public required string LastCompletionEvent { get; init; }
    public required DateTime LastUpdate { get; set; }
}