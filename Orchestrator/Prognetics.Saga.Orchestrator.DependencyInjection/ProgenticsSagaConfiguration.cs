using Microsoft.Extensions.DependencyInjection;

namespace Prognetics.Saga.Orchestrator.DependencyInjection;
public static partial class ProgneticsSagaServiceCollectionExtensions
{
    private class ProgenticsSagaConfiguration : IProgenticsSagaConfiguration
    {
        public ProgenticsSagaConfiguration(IServiceCollection services)
            => Services = services;

        public IServiceCollection Services { get; }
    }
}