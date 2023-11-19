using Prognetics.Saga.Orchestrator.Contract.DTO;

namespace Prognetics.Saga.Orchestrator.Contract;
public interface ISagaEngine
{
    Task<EngineResult<EngineOutput>> Process(
        EngineInput input,
        CancellationToken cancellationToken = default);

    Task<EngineResult<IEnumerable<EngineOutput>>> Compensate(
        string transactionId,
        CancellationToken cancellationToken = default);

    Task CompleteRollback(
        string transactionId,
        CancellationToken cancellationToken = default);
}