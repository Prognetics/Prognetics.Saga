using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Prognetics.Saga.Queue.RabbitMQ.Hosting;
class RabbitMqSagaBackgroundService : BackgroundService
{
    private readonly IRabbitMqSagaHostingService _rabbitMqSagaHostedService;
    private readonly ILogger<RabbitMqSagaBackgroundService> _logger;

    public RabbitMqSagaBackgroundService(
        IRabbitMqSagaHostingService rabbitMqSagaHostedService,
        ILogger<RabbitMqSagaBackgroundService> logger)
    {
        _rabbitMqSagaHostedService = rabbitMqSagaHostedService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _rabbitMqSagaHostedService.Listen(cancellationToken);
        }
        catch (OperationCanceledException)
        { }
    }
}
