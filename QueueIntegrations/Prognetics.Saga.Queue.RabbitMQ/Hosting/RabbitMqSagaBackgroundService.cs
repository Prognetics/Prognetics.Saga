using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prognetics.Saga.Orchestrator;
using Prognetics.Saga.Queue.RabbitMQ.Configuration;

namespace Prognetics.Saga.Queue.RabbitMQ.Hosting;
class RabbitMQSagaBackgroundService : BackgroundService
{
    private readonly IRabbitMQSagaHost _rabbitMqSagaHost;

    public RabbitMQSagaBackgroundService(
        IOptions<RabbitMQSagaOptions> options,
        IOptions<SagaModel> sagaModel,
        ILogger<IRabbitMQSagaHost> logger)
    {
        _rabbitMqSagaHost = new RabbitMQSagaHostBuilder()
            .With(options.Value)
            .With(logger)
            .Build(sagaModel.Value);
    }

    protected override Task ExecuteAsync(
        CancellationToken cancellationToken)
    {
        _rabbitMqSagaHost.Start();
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _rabbitMqSagaHost.Dispose();
        base.Dispose();
    }
}
