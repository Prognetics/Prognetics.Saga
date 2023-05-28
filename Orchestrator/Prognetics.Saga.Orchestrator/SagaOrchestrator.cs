using Prognetics.Saga.Core.Model;
using Prognetics.Saga.Orchestrator.Contract;
using Prognetics.Saga.Orchestrator.Contract.DTO;

namespace Prognetics.Saga.Orchestrator;

public class SagaOrchestrator : IStartableSagaOrchestrator
{
    private readonly SagaOptions _sagaOptions;
    private ISagaSubscriber? _sagaSubscriber;
    private SagaModel? _sagaModel;
    private ISagaEngine _engine;

    public bool IsStarted { get; private set; }

    public SagaOrchestrator(
        SagaOptions sagaOptions,
        ISagaEngine engine) =>
        (_sagaOptions, _engine) = (sagaOptions, engine);

    public void Start(ISagaSubscriber subscriber)
    {
        _sagaSubscriber = subscriber;
        IsStarted = true;
    }

    public async Task Push(string queueName, InputMessage inputMessage)
    {
        if (!IsStarted || _sagaSubscriber is null){
            throw new InvalidOperationException("Orchestrator have not been started");
        }

        if (string.Equals(
            queueName,
            _sagaOptions.ErrorQueueName,
            StringComparison.OrdinalIgnoreCase))
        {
            if (inputMessage.TransactionId is null)
            {
                // Log warnning
                return;
            }

            var compensations = await _engine.Compensate(inputMessage.TransactionId);
            await Task.WhenAll(
                compensations.Select(x =>
                    _sagaSubscriber.OnMessage(x.EventName, x.Message)));
            return;
        }

        var output = await _engine.Process(new(queueName, inputMessage));
        if(output.HasValue)
        {
            await _sagaSubscriber.OnMessage(
                output.Value.EventName,
                output.Value.Message);
        }

    }
}