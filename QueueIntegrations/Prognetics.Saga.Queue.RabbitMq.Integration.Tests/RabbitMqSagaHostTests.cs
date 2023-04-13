using Polly;
using Polly.Retry;
using Prognetics.Saga.Orchestrator;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Prognetics.Saga.Queue.RabbitMq.Integration.Tests;
public sealed class RabbitMqSagaHostTests : IClassFixture<RabbitMqContainerFixture>, IDisposable
{
    private readonly RabbitMqContainerFixture _fixture;
    private readonly RabbitMqSagaHostTestsBuilder _builder = new();
    private readonly RetryPolicy<BasicGetResult?> _gettingRetryPolicy = Policy
        .HandleResult<BasicGetResult?>(x => x == null)
            .WaitAndRetry(10, _ => TimeSpan.FromMilliseconds(100));
    private readonly IModel _channel;
    private readonly IBasicProperties _properties;
    private readonly IConnection _connection;

    public RabbitMqSagaHostTests(RabbitMqContainerFixture fixture)
    {
        _fixture = fixture;
        _builder.RabbitMqSagaOptions.ConnectionString = fixture.Container.GetConnectionString();
        _connection = new ConnectionFactory
        {
            Uri = new Uri(_builder.RabbitMqSagaOptions.ConnectionString),
            DispatchConsumersAsync = _builder.RabbitMqSagaOptions.DispatchConsumersAsync
        }.CreateConnection();
        _channel = _connection.CreateModel();
        _properties = _channel.CreateBasicProperties();
        _properties.ContentType = _builder.RabbitMqSagaOptions.ContentType;
    }

    [Fact]
    public void WhenValidMessageWasSent_ThenAppropriateMessageShouldBeFetched()
    {
        const string queueSource = $"{nameof(WhenValidMessageWasSent_ThenAppropriateMessageShouldBeFetched)}_{nameof(queueSource)}";
        const string queueTarget = $"{nameof(WhenValidMessageWasSent_ThenAppropriateMessageShouldBeFetched)}_{nameof(queueTarget)}";

        _builder.SagaModel = new SagaModelBuilder()
            .AddTransaction(x => x.AddStep(queueSource, queueTarget))
            .Build();

        var data = new TestData("Value");
        var messageTransactionId = Guid.NewGuid().ToString();
        var inputMessage = new InputMessage(
            messageTransactionId,
            queueSource,
            data,
            null);

        (var sut, var dependencies) = _builder.Build();
        var messageBytes = dependencies.RabbitMqSagaSerializer.Serialize(inputMessage);

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
        var message = dependencies.RabbitMqSagaSerializer.Deserialize<OutputMessage>(result.Body);
        Assert.NotNull(message);
        Assert.Equal(messageTransactionId, message?.TransactionId);
        Assert.Equal(queueTarget, message?.Name);
        Assert.Equal(data, ((JsonElement)message!.Payload).Deserialize<TestData>());
    }

    [Fact]
    public void IfExceptionOccurInOrchestrator_ThenExceptionShouldBeLogged()
    {
        const string queueSource = $"{nameof(IfExceptionOccurInOrchestrator_ThenExceptionShouldBeLogged)}_{nameof(queueSource)}";
        const string queueTarget = $"{nameof(IfExceptionOccurInOrchestrator_ThenExceptionShouldBeLogged)}_{nameof(queueTarget)}";
        Exception? exception = null;

        _builder.SagaModel = new SagaModelBuilder()
            .AddTransaction(x => x.AddStep(queueSource, queueTarget))
            .Build();

        _builder.OnExceptionHandler = (sender, e) =>
        {
            exception = e.Exception;
        };

        (var sut, var dependencies) = _builder.Build();
        
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
        Assert.NotNull(exception);
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}