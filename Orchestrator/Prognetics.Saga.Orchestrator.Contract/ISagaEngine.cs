using Prognetics.Saga.Orchestrator.Contract.DTO;

namespace Prognetics.Saga.Orchestrator.Contract;
public interface ISagaEngine
{
    Task<EngineResult> Process(EngineInput input);
}