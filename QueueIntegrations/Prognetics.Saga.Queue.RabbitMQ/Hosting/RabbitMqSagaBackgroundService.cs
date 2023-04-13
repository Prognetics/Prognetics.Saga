using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prognetics.Saga.Orchestrator;
using Prognetics.Saga.Queue.RabbitMQ.Configuration;

namespace Prognetics.Saga.Queue.RabbitMQ.Hosting;
class RabbitMqSagaBackgroundService : BackgroundService
{
    private readonly IRabbitMqSagaHost _rabbitMqSagaHost;

    public RabbitMqSagaBackgroundService(
        IOptions<RabbitMqSagaOptions> options,
        IOptions<SagaModel> sagaModel,
        ILogger<IRabbitMqSagaHost> logger)
    {
        _rabbitMqSagaHost = new RabbitMqSagaHostBuilder()
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
