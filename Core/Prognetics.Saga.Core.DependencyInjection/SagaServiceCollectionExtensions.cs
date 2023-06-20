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

        serviceCollection.AddScoped<IInitializableTransactionLedgerAccessor, TransactionLedgerAccessor>();
        serviceCollection.AddScoped<ITransactionLedgerAccessor>(sp => sp.GetRequiredService<IInitializableTransactionLedgerAccessor>());
        serviceCollection.AddScoped<IStartableSagaOrchestrator, SagaOrchestrator>();
        serviceCollection.AddScoped<ISagaOrchestrator>(sp => sp.GetRequiredService<IStartableSagaOrchestrator>());
        
        serviceCollection.AddScoped<IIdentifierService, IdentifierService>();
        serviceCollection.AddScoped<ISagaLog, SagaLog>();
        serviceCollection.AddScoped<ICompensationStore, CompensationStore>();
        serviceCollection.AddScoped<ISagaEngine, SagaEngine>();
        serviceCollection.AddScoped<ISagaHost, SagaHost>();

        serviceCollection.AddHostedService<SagaBackgroundService>();
        return serviceCollection;
    }
}