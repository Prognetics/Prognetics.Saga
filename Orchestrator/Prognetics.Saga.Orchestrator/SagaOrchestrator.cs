﻿namespace Prognetics.Saga.Orchestrator;

public class SagaOrchestrator : ISagaOrchestrator
{
    private readonly IReadOnlyDictionary<string, string> _steps;
    private ISagaSubscriber? _sagaSubscriber;

    public SagaOrchestrator(ISagaModelProvider provider)
    {
        _steps = provider.Model.Transactions
            .SelectMany(x => x.Steps)
            .ToDictionary(
                x => x.From,
                x => x.To);
    }

    public Task Push(string queueName, InputMessage inputMessage)
        => _sagaSubscriber?.OnMessage(
             _steps.TryGetValue(queueName, out var nextStep)
                ? nextStep
                : throw new ArgumentException("Wrong input message", nameof(inputMessage)),
            new OutputMessage(
                inputMessage.TransactionId ?? Guid.NewGuid().ToString(),
                inputMessage.Payload))
        ?? Task.CompletedTask;

    public void Subscribe(ISagaSubscriber sagaSubscriber)
    {
        _sagaSubscriber = sagaSubscriber;
    }
}