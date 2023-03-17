namespace Prognetics.Saga.Queue.RabbitMQ.Hosting;

public interface IRabbitMqSagaHostingService
{
    Task Listen(CancellationToken cancellationToken);
}
