using Microsoft.Extensions.DependencyInjection;

namespace Prognetics.Saga.Core.DependencyInjection;
public static partial class SagaServiceCollectionExtensions
{
    private class SagaConfiguration : ISagaConfiguration
    {
        public SagaConfiguration(IServiceCollection services)
            => Services = services;

        public IServiceCollection Services { get; }
    }
}