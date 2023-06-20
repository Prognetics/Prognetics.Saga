using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Prognetics.Saga.Orchestrator.Contract;
using Prognetics.Saga.Core.DependencyInjection;
using Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;
using Prognetics.Saga.Queue.RabbitMQ.Configuration;
using Prognetics.Saga.Queue.RabbitMQ.Consuming;
using Prognetics.Saga.Queue.RabbitMQ.Hosting;
using Prognetics.Saga.Queue.RabbitMQ.Serialization;
using Prognetics.Saga.Queue.RabbitMQ.Subscribing;

namespace Prognetics.Saga.Queue.RabbitMQ.DependencyInjection;
public static class SagaRabbitMQConfigurationExtensions
{
    public static ISagaConfiguration UseRabbitMQ(
        this ISagaConfiguration configuration,
        string connectionString)
        => UseRabbitMQ(
            configuration,
            x => x.ConnectionString = connectionString);

    public static ISagaConfiguration UseRabbitMQ(
        this ISagaConfiguration configuration,
        RabbitMQSagaOptions options)
        => UseRabbitMQ(
            configuration,
            x => {
                x.ConnectionString = options.ConnectionString;
                x.ContentType = options.ContentType;
                x.Exchange = options.Exchange;
                x.DispatchConsumersAsync = options.DispatchConsumersAsync;
            });

    public static ISagaConfiguration UseRabbitMQ(
        this ISagaConfiguration configuration,
        Action<RabbitMQSagaOptions> configureOptions)
    {
        configuration.Services.Configure(configureOptions);
        configuration.Services.AddTransient(x => x.GetRequiredService<IOptions<RabbitMQSagaOptions>>().Value);
        configuration.Services.AddScoped<IRabbitMQSagaSerializer, RabbitMQSagaJsonSerializer>();
        configuration.Services.AddScoped<IRabbitMQConnectionFactory, RabbitMQConnectionFactory>();
        configuration.Services.AddScoped<IRabbitMQQueuesProvider, RabbitMQQueuesProvider>();
        configuration.Services.AddScoped<IRabbitMQConsumersFactory, RabbitMQConsumersFactory>();
        configuration.Services.AddScoped<IRabbitMQConsumerFactory>(serviceProvider =>
            serviceProvider
                .GetRequiredService<IOptions<RabbitMQSagaOptions>>().Value
                .DispatchConsumersAsync
                ? new RabbitMQAsyncConsumerFactory(serviceProvider.GetRequiredService<IRabbitMQSagaSerializer>())
                : new RabbitMQConsumerFactory(serviceProvider.GetRequiredService<IRabbitMQSagaSerializer>()));
        configuration.Services.AddScoped<IRabbitMQSagaSubscriberFactory, RabbitMQSagaSubscriberFactory>();
        configuration.Services.AddScoped<ISagaClient, RabbitMQSagaClient>();
        return configuration;
    }
}
