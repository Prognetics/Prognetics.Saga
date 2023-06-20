using Prognetics.Saga.Core.Model;
using Prognetics.Saga.Orchestrator.Contract;
using Prognetics.Saga.Orchestrator.Contract.DTO;

namespace Prognetics.Saga.Orchestrator;

public class SagaOrchestrator : IStartableSagaOrchestrator
{
    private ISagaSubscriber? _sagaSubscriber;
    private readonly ISagaEngine _engine;

    public bool IsStarted { get; private set; }

    public SagaOrchestrator(ISagaEngine engine) =>
        _engine = engine;

    public void Start(ISagaSubscriber subscriber)
    {
        _sagaSubscriber = subscriber;
        IsStarted = true;
    }

    public async Task Push(string eventName, InputMessage inputMessage)
    {
        if (!IsStarted || _sagaSubscriber is null){
            throw new InvalidOperationException("Orchestrator have not been started");
        }

        var output = await _engine.Process(new(eventName, inputMessage));
        if(output.HasValue)
        {
            await _sagaSubscriber.OnMessage(
                output.Value.EventName,
                output.Value.Message);
        }
    }

    public async Task Rollback(string transactionId)
    {
        if (!IsStarted || _sagaSubscriber is null)
        {
            throw new InvalidOperationException("Orchestrator have not been started");
        }

        var compensations = await _engine.Compensate(transactionId);
        await Task.WhenAll(
            compensations.Select(x =>
                _sagaSubscriber.OnMessage(x.EventName, x.Message)));
        return;
    }
}