namespace Prognetics.Saga.Core.Model;

public record TransactionLog(string TransactionId,
    TransactionState State,
    string LastCompletionEvent,
    DateTime LastUpdate);