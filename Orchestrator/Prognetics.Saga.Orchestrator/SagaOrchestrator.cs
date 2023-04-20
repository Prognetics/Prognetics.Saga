namespace Prognetics.Saga.Orchestrator;

public class SagaOrchestrator : ISagaOrchestrator
{
    private readonly IReadOnlyDictionary<string, string> _steps;
    private readonly List<ISagaSubscriber> _sagaSubscribers = new();

    public SagaOrchestrator(ISagaModelProvider provider)
    {
        _steps = provider.Model.Transactions
            .SelectMany(x => x.Steps)
            .ToDictionary(
                x => x.From,
                x => x.To);
    }

    public Task Push(string queueName, InputMessage inputMessage)
        => _steps.TryGetValue(queueName, out var nextStep)
            ? Task.WhenAll(_sagaSubscribers.Select(s => s.OnMessage(
                nextStep,
                new OutputMessage(
                    inputMessage.TransactionId ?? Guid.NewGuid().ToString(),
                    inputMessage.Payload))))
            : throw new ArgumentException("Queue name not defined", nameof(queueName));

    public void Subscribe(ISagaSubscriber sagaSubscriber)
    {
        _sagaSubscribers.Add(sagaSubscriber);
    }
}