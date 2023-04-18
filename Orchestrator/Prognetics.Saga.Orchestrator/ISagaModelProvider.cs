namespace Prognetics.Saga.Orchestrator;

public interface ISagaModelSource
{
    SagaModel Get();
}

public interface ISagaModelProvider
{
    SagaModel Model { get; }
}

public class DelegateSagaModelSource : ISagaModelSource
{
    private readonly Func<SagaModel> _sagaModelFactory;

    public DelegateSagaModelSource(Func<SagaModel> sagaModelFactory)
    {
        _sagaModelFactory = sagaModelFactory;
    }

    public SagaModel Get() => _sagaModelFactory();
}

public class EmptySagaModelProvider : ISagaModelProvider
{
    private EmptySagaModelProvider()
    { }

    public static EmptySagaModelProvider Instance { get; } = new EmptySagaModelProvider();

    public SagaModel Model { get; } = new();
}

public class SagaModelProviderBuilder
{
    private readonly IList<ISagaModelSource> _sources = new List<ISagaModelSource>();

    public SagaModelProviderBuilder With(ISagaModelSource source)
    {
        _sources.Add(source);
        return this;
    }

    public ISagaModelProvider Build() => _sources.Any()
        ? new CompositeSagaModelProvider(_sources.ToArray())
        : EmptySagaModelProvider.Instance;
}

public class CompositeSagaModelProvider : ISagaModelProvider
{
    private readonly Lazy<SagaModel> _model;

    public CompositeSagaModelProvider(params ISagaModelSource[] sources)
    {
        _model = new Lazy<SagaModel>(() =>
        {
            var builder = new SagaModelBuilder();

            foreach (var source in sources)
            {
                builder.From(source.Get());
            }

            return builder.Build();
        });
    }

    public SagaModel Model => _model.Value;
}