using Microsoft.Extensions.DependencyInjection;
using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Orchestrator;
using Prognetics.Saga.Orchestrator.Contract;

namespace Prognetics.Saga.Core.DependencyInjection;
public static partial class SagaServiceCollectionExtensions
{
    public static IServiceCollection AddSaga(
        this IServiceCollection serviceCollection,
        Action<ISagaConfiguration> configure)
    {
        var configuration = new SagaConfiguration(serviceCollection);
        configure(configuration);

        serviceCollection.AddSingleton<IInitializableTransactionLedgerAccessor, TransactionLedgerAccessor>();
        serviceCollection.AddSingleton<ITransactionLedgerAccessor>(sp => sp.GetRequiredService<IInitializableTransactionLedgerAccessor>());
        serviceCollection.AddScoped<IStartableSagaOrchestrator, SagaOrchestrator>();
        serviceCollection.AddTransient<ISagaOrchestrator>(sp => sp.GetRequiredService<IStartableSagaOrchestrator>());
        serviceCollection.AddSingleton<IdGenerator>(() => Guid.NewGuid().ToString());
        
        serviceCollection.AddSingleton<ISagaEngine, SagaEngine>();

        serviceCollection.AddHostedService<SagaBackgroundService>();
        return serviceCollection;
    }

    public static ISagaConfiguration AddTransactionLedgerSource<TSagaModelSource>(
        this ISagaConfiguration configuration)
        where TSagaModelSource : class, ITransactionLedgerSource
    {
        configuration.Services.AddScoped<ITransactionLedgerSource, TSagaModelSource>();
        return configuration;
    }

    public static ISagaConfiguration AddSagaClient<TSagaClient>(
        this ISagaConfiguration configuration)
        where TSagaClient : class, ISagaClient
    {
        configuration.Services.AddScoped<ISagaClient, TSagaClient>();
        return configuration;
    }
}