using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly.Retry;
using Polly;
using Prognetics.Saga.Orchestrator;
using Prognetics.Saga.Queue.RabbitMQ.Configuration;
using Prognetics.Saga.Queue.RabbitMQ.Hosting;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;

namespace Prognetics.Saga.Queue.RabbitMQ.Integration.Tests;
public sealed class RabbitMQSagaBackgroundServiceTests :
    IClassFixture<RabbitMQContainerFixture>,
    IDisposable
{
    private readonly RabbitMQContainerFixture _fixture;
    private readonly RabbitMQSagaHostBuilder _hostBuilder = new();
    private readonly SagaModelBuilder _sagaModelBuilder = new();
    private readonly RabbitMQSagaOptions _options = new();
    private readonly RetryPolicy<BasicGetResult?> _gettingRetryPolicy = Policy
        .HandleResult<BasicGetResult?>(x => x == null)
        .WaitAndRetry(10, _ => TimeSpan.FromMilliseconds(100));
    private readonly IModel _channel;
    private readonly IBasicProperties _properties;
    private readonly IConnection _connection;
    private readonly IServiceCollection _services = new ServiceCollection();

    public RabbitMQSagaBackgroundServiceTests(RabbitMQContainerFixture fixture)
    {
        _fixture = fixture;
        _options.ConnectionString = fixture.Container.GetConnectionString();
        _hostBuilder.With(_options);

        _connection = new ConnectionFactory
        {
            Uri = new Uri(_options.ConnectionString),
            DispatchConsumersAsync = _options.DispatchConsumersAsync
        }.CreateConnection();

        _services.AddLogging();
        _channel = _connection.CreateModel();
        _properties = _channel.CreateBasicProperties();
        _properties.ContentType = _options.ContentType;
    }

    [Fact]
    public async Task WhenValidMessageWasSent_ThenAppropriateMessageShouldBeFetched()
    {
        const string queueSource = $"{nameof(WhenValidMessageWasSent_ThenAppropriateMessageShouldBeFetched)}_{nameof(queueSource)}";
        const string queueTarget = $"{nameof(WhenValidMessageWasSent_ThenAppropriateMessageShouldBeFetched)}_{nameof(queueTarget)}";

        var sagaModel = _sagaModelBuilder
            .AddTransaction(x => x
                .AddStep(queueSource, queueTarget))
            .Build();

        var data = new TestData("Value");
        var messageTransactionId = Guid.NewGuid().ToString();
        var inputMessage = new InputMessage(
            messageTransactionId,
            data,
            null);

        var messageBytes = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(inputMessage));

        _services.Configure<SagaModel>(x =>
            x.Transactions = sagaModel.Transactions);
        _services.Configure<RabbitMQSagaOptions>(x =>
            x.ConnectionString = _fixture.Container.GetConnectionString());
        _services.AddHostedService<RabbitMQSagaBackgroundService>();

        var serviceProvider = _services.BuildServiceProvider();

        var backgroundQueue = serviceProvider.GetRequiredService<IHostedService>()
            as RabbitMQSagaBackgroundService;

        // Act
        await backgroundQueue!.StartAsync(CancellationToken.None);

        _channel.BasicPublish(
            string.Empty,
            queueSource,
            _properties,
            messageBytes);

        var result = _gettingRetryPolicy.Execute(() =>
            _channel.BasicGet(queueTarget, true));

        // Assert
        Assert.NotNull(result);
        var message = JsonSerializer.Deserialize<OutputMessage>(Encoding.UTF8.GetString(result.Body.Span));
        Assert.NotNull(message);
        Assert.Equal(messageTransactionId, message?.TransactionId);
        Assert.Equal(data, ((JsonElement)message!.Payload).Deserialize<TestData>());
    }

    [Fact]
    public async Task IfMessegeIsSentInWrongFormat_ThenNoMessageShouldBeSend()
    {
        const string queueSource = $"{nameof(IfMessegeIsSentInWrongFormat_ThenNoMessageShouldBeSend)}_{nameof(queueSource)}";
        const string queueTarget = $"{nameof(IfMessegeIsSentInWrongFormat_ThenNoMessageShouldBeSend)}_{nameof(queueTarget)}";

        var sagaModel = _sagaModelBuilder
            .AddTransaction(x => x
                .AddStep(queueSource, queueTarget))
            .Build();

        var data = new TestData("Value");
        var messageTransactionId = Guid.NewGuid().ToString();
        var inputMessage = new InputMessage(
            messageTransactionId,
            data,
            null);

        var messageBytes = Encoding.UTF8.GetBytes(inputMessage.ToString());

        _services.Configure<SagaModel>(x =>
            x.Transactions = sagaModel.Transactions);
        _services.Configure<RabbitMQSagaOptions>(x =>
            x.ConnectionString = _fixture.Container.GetConnectionString());
        _services.AddHostedService<RabbitMQSagaBackgroundService>();

        var serviceProvider = _services.BuildServiceProvider();

        var backgroundQueue = serviceProvider.GetRequiredService<IHostedService>()
            as RabbitMQSagaBackgroundService;

        // Act
        await backgroundQueue!.StartAsync(CancellationToken.None);

        _channel.BasicPublish(
            string.Empty,
            queueSource,
            _properties,
            messageBytes);

        var result = _gettingRetryPolicy.Execute(() =>
            _channel.BasicGet(queueTarget, true));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task IfMessageIsNotKnown_ThenMessageShouldNotBeSent()
    {
        const string queueSource = $"{nameof(IfMessegeIsSentInWrongFormat_ThenNoMessageShouldBeSend)}_{nameof(queueSource)}";
        const string queueTarget = $"{nameof(IfMessegeIsSentInWrongFormat_ThenNoMessageShouldBeSend)}_{nameof(queueTarget)}";

        var sagaModel = _sagaModelBuilder
            .AddTransaction(x => x
                .AddStep(queueSource, queueTarget))
            .Build();

        var sut = _hostBuilder.Build(sagaModel);

        var data = new TestData("Value");
        var messageTransactionId = Guid.NewGuid().ToString();
        var inputMessage = new InputMessage(
            messageTransactionId,
            data,
            null);

        var messageBytes = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(inputMessage));

        _services.Configure<SagaModel>(x =>
            x.Transactions = sagaModel.Transactions);
        _services.Configure<RabbitMQSagaOptions>(x =>
            x.ConnectionString = _fixture.Container.GetConnectionString());
        _services.AddHostedService<RabbitMQSagaBackgroundService>();

        var serviceProvider = _services.BuildServiceProvider();

        var backgroundQueue = serviceProvider.GetRequiredService<IHostedService>()
            as RabbitMQSagaBackgroundService;

        // Act
        await backgroundQueue!.StartAsync(CancellationToken.None);

        _channel.BasicPublish(
            string.Empty,
            "NotKnownMessageName",
            _properties,
            messageBytes);

        var result = _gettingRetryPolicy.Execute(() =>
            _channel.BasicGet(queueTarget, true));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void WhenContentTypeIsUnknown_ThenExceptionShouldBeThrownDuringResolvingTheHostedService()
    {
        // Arrange
        _services.Configure<RabbitMQSagaOptions>(x =>
        {
            x.ConnectionString = _fixture.Container.GetConnectionString();
            x.ContentType = "Unknown";
        });

        _services.AddHostedService<RabbitMQSagaBackgroundService>();

        var serviceProvider = _services.BuildServiceProvider();

        // Act
        var resolving = () => { serviceProvider.GetRequiredService<IHostedService>(); };

        // Assert
        Assert.Throws<NotSupportedException>(resolving);
    }

    [Fact]
    public async Task WhenConnectionStringIsInvalid_ThenExceptionShouldBeThrownDuringStart()
    {
        // Arrange
        _services.Configure<RabbitMQSagaOptions>(x =>
        {
            x.ConnectionString = "amqp://user:pass@host:10000/vhost";
        });

        _services.AddHostedService<RabbitMQSagaBackgroundService>();

        var serviceProvider = _services.BuildServiceProvider();

        var backgroundQueue = serviceProvider.GetRequiredService<IHostedService>()
            as RabbitMQSagaBackgroundService;

        // Act
        var starting = () => backgroundQueue!.StartAsync(CancellationToken.None);

        // Assert
        await Assert.ThrowsAnyAsync<Exception>(starting);
    }

    public void Dispose()
    {
        _connection.Dispose();
        _channel.Dispose();
    }
}
