namespace Prognetics.Saga.Orchestrator;

public interface ISagaModelSource
{
    SagaModel Get();
}

public interface ISagaModelProvider
{
    SagaModel Model { get; }
}


public class EmptySagaModelProvider : ISagaModelProvider
{
    public SagaModel Model { get; } = new();
}

public class CompositeSagaModelProvider : ISagaModelProvider
{
    public CompositeSagaModelProvider(IEnumerable<ISagaModelSource> sources)
    {
        var builder = new SagaModelBuilder();

        foreach (var source in sources)
        {
            builder.From(source.Get());
        }

        Model = builder.Build();
    }

    public SagaModel Model { get; }
}