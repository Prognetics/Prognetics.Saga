namespace Prognetics.Saga.Orchestrator.Model;

public class CompositeSagaModelProvider : ISagaModelProvider
{
    private readonly IReadOnlyList<ISagaModelSource> _sources;
    private SagaModel? _model;

    public CompositeSagaModelProvider(IEnumerable<ISagaModelSource> sources)
    {
        _sources = sources.ToList();
    }

    public async ValueTask<SagaModel> GetModel()
        => _model ??= await BuildModel();

    private async Task<SagaModel> BuildModel()
    {
        var builder = new SagaModelBuilder();

        var models = _sources
            .Select(s => s.GetSagaModel())
            .ToList();

        foreach (var model in models)
        {
            builder.From(await model);
        }

        return builder.Build();
    }
}