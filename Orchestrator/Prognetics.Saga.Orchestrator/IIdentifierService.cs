namespace Prognetics.Saga.Orchestrator;
public interface IIdentifierService
{
    string Generate();
}

public class IdentifierService : IIdentifierService
{
    public string Generate() => Guid.NewGuid().ToString();
}