namespace Prognetics.Saga.Orchestrator;

public delegate SagaModel SagaModelSource();

public interface ISagaModelProvider
{
    SagaModel Model { get; }
}

public class EmptySagaModelProvider : ISagaModelProvider
{
    private EmptySagaModelProvider()
    { }

    public static EmptySagaModelProvider Instance { get; } = new EmptySagaModelProvider();

    public SagaModel Model { get; } = new();
}

public class CompositeSagaModelProvider : ISagaModelProvider
{
    public CompositeSagaModelProvider(IEnumerable<SagaModelSource> sources)
    {
        var builder = new SagaModelBuilder();

        foreach (var source in sources)
        {
            builder.From(source());
        }

        Model = builder.Build();
    }

    public SagaModel Model { get; }
}