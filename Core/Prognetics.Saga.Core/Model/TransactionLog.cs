namespace Prognetics.Saga.Core.Model;

public record TransactionLog
{
    public required string TransactionId { get; init; }
    public required TransactionState State { get; init; }
    public string? LastCompletionEvent { get; init; }
}