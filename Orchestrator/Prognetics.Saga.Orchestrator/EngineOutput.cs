using Prognetics.Saga.Orchestrator.Contract.DTO;

namespace Prognetics.Saga.Orchestrator;

public readonly record struct EngineOutput(
    string EventName,
    OutputMessage Message);