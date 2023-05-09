using Prognetics.Saga.Core.Model;
using Prognetics.Saga.Orchestrator.Contract;
using Prognetics.Saga.Orchestrator.Contract.DTO;
using System.Collections.Concurrent;

namespace Prognetics.Saga.Orchestrator;

public class SagaOrchestrator : ISagaOrchestrator
{
    private readonly IReadOnlyDictionary<string, string> _steps;
    private readonly ConcurrentBag<ISagaSubscriber> _sagaSubscribers = new();
    private readonly SagaModel _sagaModel;

    public SagaModel Model => _sagaModel;

    public SagaOrchestrator(SagaModel sagaModel)
    {
        _sagaModel = sagaModel;
        _steps = GetSteps();
    }

    public async Task Push(string queueName, InputMessage inputMessage)
    {
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

    private IReadOnlyDictionary<string, string> GetSteps()
        => _sagaModel.Transactions
            .SelectMany(x => x.Steps)
            .ToDictionary(
                x => x.From,
                x => x.To);

    public void Dispose()
    {
        _sagaSubscribers.Clear();
    }
}