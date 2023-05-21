using Microsoft.Extensions.DependencyInjection;
using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Orchestrator.Contract;

namespace Prognetics.Saga.Orchestrator.DependencyInjection;
public static partial class SagaServiceCollectionExtensions
{
    public static IServiceCollection AddSaga(
        this IServiceCollection serviceCollection,
        Action<ISagaConfiguration> configure)
    {
        var configuration = new SagaConfiguration(serviceCollection);
        configure(configuration);

        serviceCollection.AddScoped<ISagaOrchestrator, SagaOrchestrator>();
        serviceCollection.AddScoped<ISagaHost, SagaHost>();

        serviceCollection.AddHostedService<SagaBackgroundService>();
        return serviceCollection;
    }
}