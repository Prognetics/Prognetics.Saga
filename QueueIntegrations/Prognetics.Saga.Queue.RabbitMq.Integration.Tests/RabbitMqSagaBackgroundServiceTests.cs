using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly.Retry;
using Polly;
using Prognetics.Saga.Orchestrator;
using Prognetics.Saga.Queue.RabbitMQ.Configuration;
using Prognetics.Saga.Queue.RabbitMQ.Hosting;
using RabbitMQ.Client;
using Prognetics.Saga.Queue.RabbitMQ.Serialization;

namespace Prognetics.Saga.Queue.RabbitMq.Integration.Tests;
public sealed class RabbitMqSagaBackgroundServiceTests :
    IClassFixture<RabbitMqContainerFixture>,
    IDisposable
{
    private readonly RabbitMqContainerFixture _fixture;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IBasicProperties _properties;
    private readonly RetryPolicy<BasicGetResult?> _gettingRetryPolicy = Policy
        .HandleResult<BasicGetResult?>(x => x == null)
        .WaitAndRetry(10, _ => TimeSpan.FromMilliseconds(100));
    private readonly IServiceCollection _services;

    public RabbitMqSagaBackgroundServiceTests(RabbitMqContainerFixture fixture)
    {
        _fixture = fixture;
        _connection = new ConnectionFactory
        {
            Uri = new Uri(_fixture.Container.GetConnectionString()),
            DispatchConsumersAsync = true,
        }.CreateConnection();

        _channel = _connection.CreateModel();
        _properties = _channel.CreateBasicProperties();
        _properties.ContentType = "application/json";

        _services = new ServiceCollection();
    }

    [Fact]
    public async Task IfBackgroundServiceIsRegistered_ThenTransactionShouldBeHandled()
    {
        // Arrange
        const string source = $"{nameof(IfBackgroundServiceIsRegistered_ThenTransactionShouldBeHandled)}_{nameof(source)}";
        const string target = $"{nameof(IfBackgroundServiceIsRegistered_ThenTransactionShouldBeHandled)}_{nameof(target)}";
        var sagaModel = new SagaModelBuilder()
            .AddTransaction(x => x.AddStep(source, target))
            .Build();

        var data = new TestData("Value");
        var messageTransactionId = Guid.NewGuid().ToString();
        var inputMessage = new InputMessage(
            messageTransactionId,
            source,
            data,
            null);

        var serializer = new RabbitMqSagaJsonSerializer();
        var messageBytes = serializer.Serialize(inputMessage);

        _services.Configure<SagaModel>(x => 
            x.Transactions = sagaModel.Transactions);
        _services.Configure<RabbitMqSagaOptions>(x =>
            x.ConnectionString = _fixture.Container.GetConnectionString());
        _services.AddHostedService<RabbitMqSagaBackgroundService>();

        var serviceProvider = _services.BuildServiceProvider();

        var backgroundQueue = serviceProvider.GetRequiredService<IHostedService>()
            as RabbitMqSagaBackgroundService;

        // Act
        await backgroundQueue!.StartAsync(CancellationToken.None);
        _channel.BasicPublish(
            string.Empty,
            source,
            _properties,
            messageBytes);
        var result = _gettingRetryPolicy.Execute(() => _channel.BasicGet(target, true));

        // Assert
        Assert.NotNull(result);
    }

    public void Dispose()
    {
        _connection.Dispose();
        _channel.Dispose();
    }
}
