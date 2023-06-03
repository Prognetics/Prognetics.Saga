using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Prognetics.Saga.Core.DependencyInjection;
using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Core.DependencyInjection;
public static partial class SagaServiceCollectionExtensions
{
    private class SagaConfiguration : ISagaConfiguration
    {
        public SagaConfiguration(IServiceCollection services)
            => Services = services;

        public IServiceCollection Services { get; }
    }

    public static ISagaConfiguration Configure(
        this ISagaConfiguration sagaConfiguration,
        Action<SagaOptions> configure)
    {
        sagaConfiguration.Services.Configure(configure);
        sagaConfiguration.Services.AddTransient(
            sp => sp.GetRequiredService<IOptions<SagaOptions>>().Value);
        return sagaConfiguration;
    }
}