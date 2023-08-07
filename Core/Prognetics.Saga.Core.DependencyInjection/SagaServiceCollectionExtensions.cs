using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;
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
        serviceCollection.AddSingleton<IStartableSagaOrchestrator, SagaOrchestrator>();
        serviceCollection.AddSingleton<ISagaOrchestrator>(sp => sp.GetRequiredService<IStartableSagaOrchestrator>());
        
        serviceCollection.AddSingleton<ISagaEngine, SagaEngine>();
        serviceCollection.AddSingleton<ISagaHost, SagaHost>();

        serviceCollection.AddHostedService<SagaBackgroundService>();
        return serviceCollection;
    }

    public static ISagaConfiguration AddMongoDbSagaLog(
        this ISagaConfiguration configuration,
        Action<MongoDbSagaLogOptions> configure)
    {
        configuration.Services
            .AddOptions<MongoDbSagaLogOptions>()
            .BindConfiguration("Saga.Log")
            .Configure(configure);

        configuration.Services
            .AddTransient(x => x.GetRequiredService<IOptions<MongoDbSagaLogOptions>>().Value)
            .AddSingleton<IMongoClient>(x => new MongoClient(
                x.GetRequiredService<MongoDbSagaLogOptions>().ConnectionString))
            .AddSingleton<MongoDbSagaLog>();

        return configuration;
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