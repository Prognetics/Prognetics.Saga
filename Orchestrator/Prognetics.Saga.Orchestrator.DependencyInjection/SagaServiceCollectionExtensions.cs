﻿using Microsoft.Extensions.DependencyInjection;
using Prognetics.Saga.Orchestrator.Model;

namespace Prognetics.Saga.Orchestrator.DependencyInjection;
public static partial class SagaServiceCollectionExtensions
{
    public static IServiceCollection AddSaga(
        this IServiceCollection serviceCollection,
        Action<ISagaConfiguration> configure)
    {
        var configuration = new SagaConfiguration(serviceCollection);
        configure(configuration);

        serviceCollection.AddScoped<ISagaOrchestratorFactory, SagaOrchestratorFactory>();
        serviceCollection.AddScoped<ISagaHost, SagaHost>();

        serviceCollection.AddHostedService<SagaBackgroundService>();
        return serviceCollection;
    }

    public static ISagaConfiguration AddModelSource<TSagaModelSource>(
        this ISagaConfiguration configuration)
        where TSagaModelSource : class, ISagaModelSource
    {
        configuration.Services.AddScoped<ISagaModelSource, TSagaModelSource>();
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