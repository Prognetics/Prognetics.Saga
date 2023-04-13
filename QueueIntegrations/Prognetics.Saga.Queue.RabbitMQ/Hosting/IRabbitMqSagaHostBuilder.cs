using Prognetics.Saga.Orchestrator;

namespace Prognetics.Saga.Queue.RabbitMQ.Hosting;

public interface IRabbitMqSagaHostBuilder
{
    IRabbitMqSagaHost Build(SagaModel sagaModel);
}