using Microsoft.Extensions.DependencyInjection;

namespace Prognetics.Saga.Orchestrator.DependencyInjection;

public interface ISagaConfiguration
{
    IServiceCollection Services { get; }
}