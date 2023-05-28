using Prognetics.Saga.Orchestrator.Contract.DTO;

namespace Prognetics.Saga.Orchestrator;

public readonly record struct EngineInput(
    string EventName,
    InputMessage Message);
