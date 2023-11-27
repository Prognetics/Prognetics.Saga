using Microsoft.Extensions.Logging;
using Prognetics.Saga.Orchestrator.Contract;
using Prognetics.Saga.Orchestrator.Contract.DTO;

namespace Prognetics.Saga.Orchestrator;

public class SagaOrchestrator : IStartableSagaOrchestrator
{
    private readonly ISagaEngine _engine;
    private readonly ISagaClient _sagaClient;
    private readonly ILogger<ISagaOrchestrator> _logger;
    private ISagaSubscriber? _sagaSubscriber;
    private bool _started;

    public SagaOrchestrator(
        ISagaEngine engine,
        ISagaClient sagaClient,
        ILogger<ISagaOrchestrator> logger)
    {
        _engine = engine;
        _sagaClient = sagaClient;
        _logger = logger;
    }

    public async Task Start(CancellationToken cancellationToken = default)
    {
        await _sagaClient.Initialize();
        _sagaSubscriber = await _sagaClient.GetSubscriber();
        await _sagaClient.Consume(this);
        _started = true;
    }

    public async Task Push(string eventName, InputMessage inputMessage)
    {
        if (!_started)
        {
            throw new InvalidOperationException("Orchestrator have not been started");
        }

        if (_sagaSubscriber is null)
        {
            _logger.LogWarning(
                "Attempted to push a message {EventName} to the orchestrator without a subscriber",
                eventName);
            return;
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
        if (!_started)
        {
            throw new InvalidOperationException("Orchestrator have not been started");
        }

        if (_sagaSubscriber is null)
        {
            _logger.LogWarning(
                "Attempted to rollback the process {ProcessId} without a subscriber set",
                transactionId);
            return;
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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sagaClient.Dispose();
            _sagaSubscriber = null;
        }
    }
}