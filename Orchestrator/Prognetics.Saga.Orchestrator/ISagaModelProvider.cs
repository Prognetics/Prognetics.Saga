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
    private EmptySagaModelProvider()
    { }

    public static EmptySagaModelProvider Instance { get; } = new EmptySagaModelProvider();

    public SagaModel Model { get; } = new();
}

public class CompositeSagaModelProvider : ISagaModelProvider
{
    private readonly Lazy<SagaModel> _model;

    public CompositeSagaModelProvider(IEnumerable<ISagaModelSource> sources)
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