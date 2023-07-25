using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Orchestrator.Contract;

public interface ISagaClient : IDisposable
{
    Task Initialize();

    Task Consume(ISagaOrchestrator orchestrator);

    Task<ISagaSubscriber> GetSubscriber();

}