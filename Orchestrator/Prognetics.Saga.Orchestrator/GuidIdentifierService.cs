namespace Prognetics.Saga.Orchestrator;

public class GuidIdentifierService : IIdentifierService
{
    public string Generate() => Guid.NewGuid().ToString();
}