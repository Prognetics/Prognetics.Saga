using Prognetics.Saga.Common.Model;
using Prognetics.Saga.Orchestrator.Contract;

namespace Prognetics.Saga.Orchestrator;

public class SagaOrchestratorFactory : ISagaOrchestratorFactory
{
    private readonly IReadOnlyList<ISagaModelSource> _sources;

    public SagaOrchestratorFactory(IEnumerable<ISagaModelSource> sources)
    {
        _sources = sources.ToList();
    }

    public async Task<ISagaOrchestrator> Create(CancellationToken cancellation)
    {
        var builder = new SagaModelBuilder();

        var models = _sources
            .Select(s => s.GetSagaModel())
            .ToList();

        foreach (var model in models)
        {
            builder.From(await model);
        }

        return new SagaOrchestrator(builder.Build());
    }
}