using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;
using Prognetics.Saga.Orchestrator.Contract;
using Prognetics.SagaLog.Core.Abstract;

namespace Prognetics.Saga.Orchestrator;

public class SagaOrchestratorFactory : ISagaOrchestratorFactory
{
    private readonly IReadOnlyList<IModelSource> _sources;
    private readonly ISagaLog _sagaLog;

    public SagaOrchestratorFactory(IEnumerable<IModelSource> sources, 
        ISagaLog sagaLog)
    {
        _sources = sources.ToList();
        _sagaLog = sagaLog;
    }

    public async Task<ISagaOrchestrator> Create(CancellationToken cancellation)
    {
        var builder = new ModelBuilder();

        var models = _sources
            .Select(s => s.GetModel())
            .ToList();

        foreach (var source in _sources)
        {
            source.ModelChanged += UpdateLedger;
        }

        foreach (var model in models)
        {
            builder.FromLedger(await model);
        }

        return new SagaOrchestrator(builder.Build());
    }

    private void UpdateLedger(object sender, EventArgs e)
    {
        // todo in the future this has to be implemented to replace the in memory mechanism
        _sagaLog.UpdateSagaLog();
    }
}