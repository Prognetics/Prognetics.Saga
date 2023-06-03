using Prognetics.Saga.Orchestrator.Contract.DTO;

namespace Prognetics.Saga.Orchestrator;
public interface ISagaEngine
{
    Task<EngineOutput?> Process(EngineInput input);

    Task<IEnumerable<EngineOutput>> Compensate(string transactionId);
}