namespace Prognetics.Saga.Orchestrator.Contract.DTO;

public readonly record struct EngineInput(
    string EventName,
    InputMessage Message);
