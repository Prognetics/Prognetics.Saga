using Prognetics.Saga.Orchestrator.Contract;
using Prognetics.Saga.Orchestrator.Contract.DTO;

namespace Prognetics.Saga.Orchestrator;

public class SagaOrchestrator : IStartableSagaOrchestrator
{
    private readonly ISagaEngine _engine;
    private ISagaSubscriber? _sagaSubscriber;

    public bool IsStarted { get; private set; }

    public SagaOrchestrator(ISagaEngine engine)
    {
        _engine = engine;
    }

    public void Start(ISagaSubscriber sagaSubscriber)
    {
        _sagaSubscriber = sagaSubscriber;
        IsStarted = true;
    }

    public async Task Push(string eventName, InputMessage inputMessage)
    {
        if (!IsStarted || _sagaSubscriber is null)
        {
            throw new InvalidOperationException("Orchestrator have not been started");
        }

        var result = await _engine.Process(new(eventName, inputMessage));
        if (result.TryGetOutput(out var output))
        {
            await _sagaSubscriber.OnMessage(
                output.EventName,
                output.Message);
        }
    }

    public async Task Rollback(string transactionId, CancellationToken cancellationToken = default)
    {
        if (!IsStarted || _sagaSubscriber is null)
        {
            throw new InvalidOperationException("Orchestrator have not been started");
        }

        var result = await _engine.Compensate(transactionId, cancellationToken);
        if (!result.TryGetOutput(out var compensations))
        {
            return;
        }

        await Task.WhenAll(
            compensations.Select(x =>
                _sagaSubscriber.OnMessage(x.EventName, x.Message)));

        await _engine.CompleteRollback(transactionId, cancellationToken);

    }
}