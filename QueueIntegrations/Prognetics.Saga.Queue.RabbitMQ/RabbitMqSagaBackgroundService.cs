using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Prognetics.Saga.Queue.RabbitMQ;
public class RabbitMqSagaBackgroundService : BackgroundService
{
    private readonly RabbitMqSagaHostingService _rabbitMqSagaHostedService;
    private readonly ILogger<RabbitMqSagaBackgroundService> _logger;

    public RabbitMqSagaBackgroundService(
        RabbitMqSagaHostingService rabbitMqSagaHostedService,
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
