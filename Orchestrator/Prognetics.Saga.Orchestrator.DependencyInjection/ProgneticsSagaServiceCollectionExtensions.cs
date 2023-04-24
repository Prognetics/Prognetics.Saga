using Microsoft.Extensions.DependencyInjection;
using Prognetics.Saga.Orchestrator.Model;

namespace Prognetics.Saga.Orchestrator.DependencyInjection;
public static partial class ProgneticsSagaServiceCollectionExtensions
{
    public static IServiceCollection AddProgneticsSaga(
        this IServiceCollection serviceCollection,
        Action<IProgenticsSagaConfiguration> configure)
    {
        var configuration = new ProgenticsSagaConfiguration(serviceCollection);
        configure(configuration);

        serviceCollection.AddScoped<ISagaOrchestratorFactory, SagaOrchestratorFactory>();
        serviceCollection.AddScoped<ISagaHost, SagaHost>();

        serviceCollection.AddHostedService<SagaBackgroundService>();
        return serviceCollection;
    }

    public static IProgenticsSagaConfiguration AddModelSource<TSagaModelSource>(
        this IProgenticsSagaConfiguration configuration)
        where TSagaModelSource : class, ISagaModelSource
    {
        configuration.Services.AddScoped<ISagaModelSource, TSagaModelSource>();
        return configuration;
    }

    public static IProgenticsSagaConfiguration AddSagaClient<TSagaClient>(
        this IProgenticsSagaConfiguration configuration)
        where TSagaClient : class, ISagaClient
    {
        configuration.Services.AddScoped<ISagaClient, TSagaClient>();
        return configuration;
    }
}