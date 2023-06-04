using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;
using Prognetics.Saga.Orchestrator.Contract;

namespace Prognetics.Saga.Orchestrator;

public class SagaOrchestratorFactory : ISagaOrchestratorFactory
{
    private readonly IReadOnlyList<IModelSource> _sources;

    public SagaOrchestratorFactory(IEnumerable<IModelSource> sources)
    {
        _sources = sources.ToList();
    }

    public async Task<ISagaOrchestrator> Create(CancellationToken cancellation)
    {
        var builder = new ModelBuilder();

        var models = _sources
            .Select(s => s.GetModel())
            .ToList();

        foreach (var model in models)
        {
            builder.FromLedger(await model);
        }

        return new SagaOrchestrator(builder.Build());
    }
}