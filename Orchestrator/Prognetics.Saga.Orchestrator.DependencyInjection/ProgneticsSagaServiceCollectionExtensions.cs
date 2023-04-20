using Microsoft.Extensions.DependencyInjection;

namespace Prognetics.Saga.Orchestrator.DependencyInjection;
public static partial class ProgneticsSagaServiceCollectionExtensions
{
    public static IServiceCollection AddProgneticsSaga(
        this IServiceCollection serviceCollection,
        Action<IProgenticsSagaConfiguration> configure)
    {
        var configuration = new ProgenticsSagaConfiguration(serviceCollection);
        configure(configuration);

        serviceCollection.AddSingleton<ISagaModelProvider, CompositeSagaModelProvider>();
        serviceCollection.AddSingleton<ISagaOrchestrator, SagaOrchestrator>();

        serviceCollection.AddHostedService<SagaBackgroundService>();
        return serviceCollection;
    }

    public static IProgenticsSagaConfiguration ConfigureModel(
        this IProgenticsSagaConfiguration serviceCollection,
        Action<SagaModelBuilder> configure)
        => ConfigureModel(serviceCollection, () =>
        {
            var builder = new SagaModelBuilder();
             configure(builder);
            return builder.Build();
        });

    public static IProgenticsSagaConfiguration ConfigureModel(
        this IProgenticsSagaConfiguration configuration,
        SagaModelSource factory)
    {
        configuration.Services.AddSingleton(factory);
        return configuration;
    }
}