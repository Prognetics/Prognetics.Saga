using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Prognetics.Saga.Orchestrator.DependencyInjection;
using Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;
using Prognetics.Saga.Queue.RabbitMQ.Configuration;
using Prognetics.Saga.Queue.RabbitMQ.Consuming;
using Prognetics.Saga.Queue.RabbitMQ.Hosting;
using Prognetics.Saga.Queue.RabbitMQ.Serialization;
using Prognetics.Saga.Queue.RabbitMQ.Subscribing;

namespace Prognetics.Saga.Queue.RabbitMQ.DependecyInjection;
public static class SagaRabbitMQConfigurationExtensions
{
    public static IProgenticsSagaConfiguration UseRabbitMQ(
        this IProgenticsSagaConfiguration configuration,
        string connectionString)
        => UseRabbitMQ(
            configuration,
            x => x.ConnectionString = connectionString);

    public static IProgenticsSagaConfiguration UseRabbitMQ(
        this IProgenticsSagaConfiguration configuration,
        Action<RabbitMQSagaOptions> configureOptions)
    {
        configuration.Services.Configure(configureOptions);
        configuration.Services.AddTransient(x => x.GetRequiredService<IOptions<RabbitMQSagaOptions>>().Value);
        configuration.Services.AddSingleton<IRabbitMQSagaSerializer>(serviceProvider => 
            serviceProvider
                .GetRequiredService<IOptions<RabbitMQSagaOptions>>().Value
                .ContentType switch
                {
                    "application/json" => new RabbitMQSagaJsonSerializer(),
                    _ => throw new NotSupportedException(),
                });
        configuration.Services.AddSingleton<IRabbitMQConnectionFactory, RabbitMQConnectionFactory>();
        configuration.Services.AddSingleton<IRabbitMQQueuesProvider, RabbitMQQueuesProvider>();
        configuration.Services.AddSingleton<IRabbitMQConsumersFactory, RabbitMQConsumersFactory>();
        configuration.Services.AddSingleton<IRabbitMQConsumerFactory>(serviceProvider =>
            serviceProvider
                .GetRequiredService<IOptions<RabbitMQSagaOptions>>().Value
                .DispatchConsumersAsync
                ? new RabbitMQAsyncConsumerFactory(serviceProvider.GetRequiredService<IRabbitMQSagaSerializer>())
                : new RabbitMQConsumerFactory(serviceProvider.GetRequiredService<IRabbitMQSagaSerializer>()));
        configuration.Services.AddSingleton<IRabbitMQSagaSubscriberFactory, RabbitMQSagaSubscriberFactory>();
        configuration.UseHost<RabbitMQSagaHost>();
        return configuration;
    }
}
