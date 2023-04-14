using Prognetics.Saga.Orchestrator;

namespace Prognetics.Saga.Queue.RabbitMQ.Hosting;

public interface IRabbitMQSagaHostBuilder
{
    IRabbitMQSagaHost Build(SagaModel sagaModel);
}