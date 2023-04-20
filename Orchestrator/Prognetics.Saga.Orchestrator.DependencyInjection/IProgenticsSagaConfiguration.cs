using Microsoft.Extensions.DependencyInjection;

namespace Prognetics.Saga.Orchestrator.DependencyInjection;

public interface IProgenticsSagaConfiguration
{
    IServiceCollection Services { get; }
}

