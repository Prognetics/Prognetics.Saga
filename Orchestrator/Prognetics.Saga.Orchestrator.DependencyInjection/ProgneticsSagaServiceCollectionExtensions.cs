using Microsoft.Extensions.DependencyInjection;

namespace Prognetics.Saga.Orchestrator.DependencyInjection;
public static partial class ProgneticsSagaServiceCollectionExtensions
{
    public static IServiceCollection AddProgneticsSaga(
        this IServiceCollection serviceCollection,
        Action<IProgenticsSagaConfiguration> configure)
    {
        var configuration = new ProgenticsSagaConfiguration();
        configure(configuration);

        foreach (var host in configuration.Hosts)
        {
            serviceCollection.AddTransient(typeof(ISagaHost), host);
        }

        foreach (var source in configuration.ModelSources)
        {
            serviceCollection.AddSingleton(typeof(ISagaModelSource), source);
        }

        serviceCollection.AddSingleton<ISagaModelProvider, CompositeSagaModelProvider>();
        serviceCollection.AddSingleton<ISagaOrchestrator, SagaOrchestrator>();

        serviceCollection.AddHostedService<SagaBackgroundService>();
        return serviceCollection;
    }
}