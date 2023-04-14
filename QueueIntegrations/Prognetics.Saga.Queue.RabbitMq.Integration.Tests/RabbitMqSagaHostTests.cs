using Polly;
using Polly.Retry;
using Prognetics.Saga.Orchestrator;
using Prognetics.Saga.Queue.RabbitMQ.Configuration;
using Prognetics.Saga.Queue.RabbitMQ.Hosting;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Prognetics.Saga.Queue.RabbitMq.Integration.Tests;
public sealed class RabbitMqSagaHostTests : IClassFixture<RabbitMqContainerFixture>, IDisposable
{
    private readonly RabbitMqContainerFixture _fixture;
    private readonly RabbitMqSagaHostBuilder _hostBuilder = new ();
    private readonly SagaModelBuilder _sagaModelBuilder = new ();
    private readonly RabbitMqSagaOptions _options = new ();
    private readonly RetryPolicy<BasicGetResult?> _gettingRetryPolicy = Policy
        .HandleResult<BasicGetResult?>(x => x == null)
        .WaitAndRetry(10, _ => TimeSpan.FromMilliseconds(100));
    private readonly IModel _channel;
    private readonly IBasicProperties _properties;
    private readonly IConnection _connection;

    public RabbitMqSagaHostTests(RabbitMqContainerFixture fixture)
    {
        _fixture = fixture;
        _options.ConnectionString = fixture.Container.GetConnectionString();
        _hostBuilder.With(_options);

        _connection = new ConnectionFactory
        {
            Uri = new Uri(_options.ConnectionString),
            DispatchConsumersAsync = _options.DispatchConsumersAsync
        }.CreateConnection();

        _channel = _connection.CreateModel();
        _properties = _channel.CreateBasicProperties();
        _properties.ContentType = _options.ContentType;
    }

    [Fact]
    public void WhenValidMessageWasSent_ThenAppropriateMessageShouldBeFetched()
    {
        const string queueSource = $"{nameof(WhenValidMessageWasSent_ThenAppropriateMessageShouldBeFetched)}_{nameof(queueSource)}";
        const string queueTarget = $"{nameof(WhenValidMessageWasSent_ThenAppropriateMessageShouldBeFetched)}_{nameof(queueTarget)}";

        var sagaModel = _sagaModelBuilder
            .AddTransaction(x => x
                .AddStep(queueSource, queueTarget))
            .Build();

        var sut = _hostBuilder.Build(sagaModel);

        var data = new TestData("Value");
        var messageTransactionId = Guid.NewGuid().ToString();
        var inputMessage = new InputMessage(
            messageTransactionId,
            queueSource,
            data,
            null);

        var messageBytes = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(inputMessage));

        // Act
        sut.Start();

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
        Assert.Equal(queueTarget, message?.Name);
        Assert.Equal(data, ((JsonElement)message!.Payload).Deserialize<TestData>());
    }

    [Fact]
    public void IfMessegeIsSentInWrongFormat_ThenNoMessageShouldBeSend()
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
            queueSource,
            data,
            null);

        var messageBytes = Encoding.UTF8.GetBytes(inputMessage.ToString());
        
        // Act
        sut.Start();
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
    public void IfMessageIsNotKnown_ThenMessageShouldNotBeSent()
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
            "NotKnownMessageName",
            data,
            null);

        var messageBytes = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(inputMessage));

        // Act
        sut.Start();
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
    public void WhenContentTypeIsUnknown_ThenExceptionShouldBeThrownDuringBuilding()
    {
        // Arrange
        _options.ContentType = "Unknown";

        // Act
        var building = () => { _hostBuilder.Build(new SagaModel()); };

        // Assert
        Assert.Throws<NotSupportedException>(building);
    }

    [Fact]
    public void WhenConnectionStringIsInvalid_ThenExceptionShouldBeThrownDuringStart()
    {
        // Arrange
        _options.ConnectionString = "amqp://user:pass@host:10000/vhost";
        var sut = _hostBuilder.Build(new SagaModel());

        // Act
        var starting = sut.Start;

        // Assert
        Assert.ThrowsAny<Exception>(starting);
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}