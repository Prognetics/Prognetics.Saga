namespace Prognetics.Saga.Queue.RabbitMQ.Hosting;

public interface IRabbitMQSagaHost : IDisposable
{
    void Start();
}
