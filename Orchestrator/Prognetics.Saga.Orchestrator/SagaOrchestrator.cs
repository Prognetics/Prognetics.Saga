using Prognetics.Saga.Orchestrator.DTO;
using Prognetics.Saga.Orchestrator.SagaModel;

namespace Prognetics.Saga.Orchestrator;

public class SagaOrchestrator : ISagaOrchestrator
{
    private IReadOnlyDictionary<string, string>? _steps = null;
    private readonly List<ISagaSubscriber> _sagaSubscribers = new();
    private readonly ISagaModelProvider _provider;

    public SagaOrchestrator(ISagaModelProvider provider)
    {
        _provider = provider;
    }

    public async Task Push(string queueName, InputMessage inputMessage)
    {
        _steps ??= await GetSteps();

        if (!_steps.TryGetValue(queueName, out var nextStep))
        {
            throw new ArgumentException("Queue name not defined", nameof(queueName));
        }

        var outputMessage = new OutputMessage(
            inputMessage.TransactionId ?? Guid.NewGuid().ToString(),
            inputMessage.Payload);

        await Parallel.ForEachAsync(
            _sagaSubscribers,
            (s, _) => new(s.OnMessage(
                nextStep,
                outputMessage)));
    }

    public void Subscribe(ISagaSubscriber sagaSubscriber)
    {
        _sagaSubscribers.Add(sagaSubscriber);
    }

    private async Task<IReadOnlyDictionary<string, string>> GetSteps()
        => (await _provider.GetModel()).Transactions
            .SelectMany(x => x.Steps)
            .ToDictionary(
                x => x.From,
                x => x.To);
}