﻿using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;
using Prognetics.Saga.Queue.RabbitMQ.Configuration;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Prognetics.Saga.Orchestrator.DependencyInjection;
using Prognetics.Saga.Queue.RabbitMQ.DependecyInjection;
using Microsoft.Extensions.Hosting;
using Prognetics.Saga.Orchestrator.Model;
using Prognetics.Saga.Orchestrator.DTO;

namespace Prognetics.Saga.Queue.RabbitMQ.Integration.Tests;
public sealed class RabbitMQSagaHostTests : IClassFixture<RabbitMQContainerFixture>, IDisposable
{
    private readonly RabbitMQContainerFixture _fixture;

    private readonly RabbitMQSagaOptions _options = new ();
    private readonly RetryPolicy<BasicGetResult?> _gettingRetryPolicy = Policy
        .HandleResult<BasicGetResult?>(x => x == null)
        .WaitAndRetry(10, _ => TimeSpan.FromMilliseconds(100));
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IBasicProperties _properties;

    private readonly string _queueSource;
    private readonly string _queueTarget;
    private const string _exchange = "saga";
    private readonly IServiceCollection _serviceCollection;
    private readonly IServiceProvider _serviceProvider;
    private readonly SagaBackgroundService _sut;

    public RabbitMQSagaHostTests(RabbitMQContainerFixture fixture)
    {
        _fixture = fixture;
        _connection = _fixture.Connection;
        _channel = _connection.CreateModel();
        _properties = _channel.CreateBasicProperties();
        _properties.ContentType = _options.ContentType;
        _options.ConnectionString = fixture.Container.GetConnectionString();
        _options.Exchange = _exchange;

        _queueSource = Guid.NewGuid().ToString();
        _queueTarget = Guid.NewGuid().ToString();

        _serviceCollection = new ServiceCollection()
            .AddLogging()
            .AddProgneticsSaga(config => config
                .AddModelSource<DelegateSagaModelSource>()
                .UseRabbitMQ(_options)
                .Services.AddSingleton(new Action<SagaModelBuilder>(builder =>
                    builder.AddTransaction(t => t.AddStep(_queueSource, _queueTarget)))));

        _serviceProvider = _serviceCollection.BuildServiceProvider();

        _sut = (_serviceProvider.GetRequiredService<IHostedService>() as SagaBackgroundService)!;
    }

    [Fact]
    public async Task WhenValidMessageWasSent_ThenAppropriateMessageShouldBeFetched()
    {
        var data = new TestData("Value");
        var messageTransactionId = Guid.NewGuid().ToString();
        var inputMessage = new InputMessage(
            messageTransactionId,
            data,
            null);

        var messageBytes = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(inputMessage));

        // Act
        await _sut.StartAsync(CancellationToken.None);

        _channel.BasicPublish(
            _exchange,
            _queueSource,
            _properties,
            messageBytes);

        var result = _gettingRetryPolicy.Execute(() =>
            _channel.BasicGet(_queueTarget, true));

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
        var data = new TestData("Value");
        var messageTransactionId = Guid.NewGuid().ToString();
        var inputMessage = new InputMessage(
            messageTransactionId,
            data,
            null);

        var messageBytes = Encoding.UTF8.GetBytes(inputMessage.ToString());

        // Act
        await _sut.StartAsync(CancellationToken.None);

        _channel.BasicPublish(
            _exchange,
            _queueSource,
            _properties,
            messageBytes);

        var result = _gettingRetryPolicy.Execute(() =>
            _channel.BasicGet(_queueTarget, true));
        
        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task IfMessageIsNotKnown_ThenMessageShouldNotBeSent()
    {
        var data = new TestData("Value");
        var messageTransactionId = Guid.NewGuid().ToString();
        var inputMessage = new InputMessage(
            messageTransactionId,
            data,
            null);

        var messageBytes = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(inputMessage));

        // Act
        await _sut.StartAsync(CancellationToken.None);

        _channel.BasicPublish(
            _exchange,
            "NotKnownMessageName",
            _properties,
            messageBytes);

        var result = _gettingRetryPolicy.Execute(() =>
            _channel.BasicGet(_queueTarget, true));

        // Assert
        Assert.Null(result);
    }


    [Fact]
    public void WhenConnectionStringIsInvalid_ThenExceptionShouldBeThrownDuringStart()
    {
        // Arrange
        var sut = (_serviceCollection
            .Configure<RabbitMQSagaOptions>(x => x.ConnectionString = "amqp://user:pass@host:10000/vhost")
            .BuildServiceProvider()
            .GetRequiredService<IHostedService>() as SagaBackgroundService)!;

        // Act
        var starting = () => sut.StartAsync(CancellationToken.None);

        // Assert
        Assert.ThrowsAnyAsync<Exception>(starting);
    }

    public void Dispose()
    {
        _connection.Dispose();
        _channel.Dispose();
    }
}