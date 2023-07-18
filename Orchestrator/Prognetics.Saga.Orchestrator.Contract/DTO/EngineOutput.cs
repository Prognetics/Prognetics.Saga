namespace Prognetics.Saga.Orchestrator.Contract.DTO;

public readonly record struct EngineOutput(
    string EventName,
    OutputMessage Message);