namespace Prognetics.Saga.Queue.RabbitMQ.Hosting;

public interface IRabbitMqSagaHost : IDisposable
{
    void Start();
}
