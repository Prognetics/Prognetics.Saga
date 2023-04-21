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

        serviceCollection.AddSingleton<ISagaModelProvider, CompositeSagaModelProvider>();
        serviceCollection.AddSingleton<ISagaOrchestrator, SagaOrchestrator>();

        serviceCollection.AddHostedService<SagaBackgroundService>();
        return serviceCollection;
    }

    public static IProgenticsSagaConfiguration AddModelSource(
        this IProgenticsSagaConfiguration serviceCollection,
        Action<ISagaModelBuilder> configure)
        => AddModelSource(serviceCollection, new DelegateSagaModelSource(() =>
        {
            var builder = new SagaModelBuilder();
             configure(builder);
            return builder.Build();
        }));

    public static IProgenticsSagaConfiguration AddModelSource(
        this IProgenticsSagaConfiguration configuration,
        ISagaModelSource factory)
    {
        configuration.Services.AddSingleton(factory);
        return configuration;
    }
}