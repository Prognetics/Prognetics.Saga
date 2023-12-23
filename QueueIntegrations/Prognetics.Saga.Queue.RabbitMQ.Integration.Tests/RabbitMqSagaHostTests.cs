using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;
using Prognetics.Saga.Queue.RabbitMQ.Configuration;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Prognetics.Saga.Core.DependencyInjection;
using Prognetics.Saga.Queue.RabbitMQ.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prognetics.Saga.Orchestrator.Contract.DTO;
using Prognetics.Saga.Parsers.DependencyInjection;
using Prognetics.Saga.Parsers.Core.Model;
using Microsoft.Extensions.Configuration;
using Prognetics.Saga.Log.MongoDb;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;
using Xunit.Abstractions;
using Prognetics.Saga.Core;
using MongoDB.Driver.Core.Bindings;
using static MongoDB.Driver.WriteConcern;

namespace Prognetics.Saga.Queue.RabbitMQ.Integration.Tests;
/// <summary>
/// This tests MAY fail because of poor connection between RabbitMQ container and clients.
/// </summary>
public sealed class RabbitMQSagaHostTests :
    IClassFixture<RabbitMQContainerFixture>,
    IClassFixture<MongoDbContainerFixture>,
    IDisposable
{
    private const string _skipReason = null;
    private readonly RabbitMQContainerFixture _fixture;
    private readonly MongoDbContainerFixture _mongoDbContainerFixture;
    private readonly RabbitMQSagaOptions _options = new ();
    private readonly RetryPolicy<BasicGetResult?> _gettingRetryPolicy = Policy
        .HandleResult<BasicGetResult?>(x => x == null)
        .WaitAndRetry(10, _ => TimeSpan.FromMilliseconds(100));
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IBasicProperties _properties;

    private const string _exchange = "saga";
    private readonly IServiceCollection _serviceCollection;
    private readonly IServiceProvider _serviceProvider;
    private readonly SagaBackgroundService _sut;
    private readonly CancellationTokenSource _ctsSut = new ();
    private readonly CancellationTokenSource _ctsStarted = new ();

    public RabbitMQSagaHostTests(
        RabbitMQContainerFixture fixture,
        MongoDbContainerFixture mongoDbContainerFixture,
        ITestOutputHelper testOutputHelper)
    {
        var messages = Channel.CreateBounded<string>(new BoundedChannelOptions(int.MaxValue) { SingleReader = true });

        Task.Run(async () =>
        {
            await foreach (var message in messages.Reader.ReadAllAsync())
            {
                if (message == null)
                    continue;

                testOutputHelper.WriteLine(message);
            }
        });

        _fixture = fixture;
        _mongoDbContainerFixture = mongoDbContainerFixture;
        _connection = _fixture.Connection;
        _channel = _connection.CreateModel();
        _properties = _channel.CreateBasicProperties();
        _properties.ContentType = _options.ContentType;
        _options.ConnectionString = fixture.Container.GetConnectionString();
        _options.Exchange = _exchange;

        _serviceCollection = new ServiceCollection()
            .AddSingleton<IConfiguration>(new ConfigurationBuilder().Build())
            .AddLogging(builder => builder.AddProvider(new XUnitLoggerProvider(messages)))
            .AddSaga(config => config
                .UseMongoDbSagaLog(x => x.ConnectionString = _mongoDbContainerFixture.Container.GetConnectionString())
                .UseParser(option =>
                {
                    option.Configurations = new List<ReaderConfiguration>
                    {
                        new ReaderConfiguration
                        {
                            ParserType = "Prognetics.Saga.Parser.Json.Reader.JsonFromFileTransactionLedgerReader, Prognetics.Saga.Parser.Json",
                            Path = "./TestFiles/JsonParserTestFile.json"
                        }
                    };
                })
                .UseRabbitMQ(_options));
        
        _serviceProvider = _serviceCollection.BuildServiceProvider();

        _sut = (_serviceProvider.GetRequiredService<IHostedService>() as SagaBackgroundService)!;
        _sut.OnStarted.Register(_ctsStarted.Cancel);
    }

    [Fact(Skip = _skipReason)]
    public async Task WhenValidMessagesInCorrectOrderWasSent_ThenAppropriateMessageShouldBeFetched()
    {
        await _sut.StartAsync(_ctsSut.Token);
        await _ctsStarted.Token.Wait();

        var transactionId = ExecuteTransactionStep(
            null,
            "Transaction1Step1Completion",
            "Transaction1Step1Next",
            ValidResult,
            ValidMessage);

        Assert.NotNull(transactionId);

        ExecuteTransactionStep(
            transactionId,
            "Transaction1Step2Completion",
            "Transaction1Step2Next",
            ValidResult,
            ValidMessage);

        ExecuteTransactionStep(
            transactionId,
            "Transaction1Step3Completion",
            "Transaction1Step3Next",
            ValidResult,
            ValidMessage);
    }

    [Fact(Skip = _skipReason)]
    public async Task WhenValidMessagesInIncorrectOrderWasSent_ThenOnlyAppropriateMessageShouldBeFetched()
    {
        await _sut.StartAsync(_ctsSut.Token);
        await _ctsStarted.Token.Wait();

        var transactionId = ExecuteTransactionStep(
            null,
            "Transaction1Step1Completion",
            "Transaction1Step1Next",
            ValidResult,
            ValidMessage);

        Assert.NotNull(transactionId);

        ExecuteTransactionStep(
            transactionId,
            "Transaction1Step3Completion",
            "Transaction1Step3Next",
            NullResult,
            NullMessage);

        ExecuteTransactionStep(
            transactionId,
            "Transaction1Step2Completion",
            "Transaction1Step2Next",
            ValidResult,
            ValidMessage);

        ExecuteTransactionStep(
            transactionId,
            "Transaction1Step3Completion",
            "Transaction1Step3Next",
            ValidResult,
            ValidMessage);
    }


    [Fact(Skip = _skipReason)]
    public async Task WhenStepFailed_ThenRollbackShouldBeExecuted()
    {
        await _sut.StartAsync(_ctsSut.Token);
        await _ctsStarted.Token.Wait();

        var transactionId = ExecuteTransactionStep(
            null,
            "Transaction1Step1Completion",
            "Transaction1Step1Next",
            ValidResult,
            ValidMessage);

        Assert.NotNull(transactionId);

        ExecuteTransactionStep(
            transactionId,
            "Transaction1Step2Completion",
            "Transaction1Step2Next",
            ValidResult,
            ValidMessage);

        ExecuteTransactionStep(
            transactionId,
            "Transaction1Step3Completion",
            "Transaction1Step3Next",
            ValidResult,
            ValidMessage,
            false);

        void AssertCompensation(string compensationName)
        {
            var result = _gettingRetryPolicy.Execute(() =>
                _channel.BasicGet(compensationName, true));

            Assert.NotNull(result);
        }

        AssertCompensation("Transaction1Step3Compensation");
        AssertCompensation("Transaction1Step2Compensation");
        AssertCompensation("Transaction1Step1Compensation");
    }

    [Fact(Skip = _skipReason)]
    public async Task IfMessegeIsSentInWrongFormat_ThenNoMessageShouldBeSend()
    {
        var initialEventName = "Transaction1Step2Completion";
        var initialNextEventName = "Transaction1Step2Next";
        var data = new TestData("Value");
        var messageTransactionId = Guid.NewGuid().ToString();
        var inputMessage = new InputMessage(
            messageTransactionId,
            data,
            null);

        var messageBytes = Encoding.UTF8.GetBytes(inputMessage.ToString());

        // Act
        await _sut.StartAsync(_ctsSut.Token);
        await _ctsStarted.Token.Wait();

        _channel.BasicPublish(
            _exchange,
            initialEventName,
            _properties,
            messageBytes);

        var result = _gettingRetryPolicy.Execute(() =>
            _channel.BasicGet(initialNextEventName, true));
        
        // Assert
        Assert.Null(result);
    }

    [Fact(Skip = _skipReason)]
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
        await _sut.StartAsync(_ctsSut.Token);
        await _ctsStarted.Token.Wait();
        _channel.BasicPublish(
            _exchange,
            "NotKnownMessageName",
            _properties,
            messageBytes);

        var result = _gettingRetryPolicy.Execute(() =>
            _channel.BasicGet("Transaction1Step2Next", true));

        // Assert
        Assert.Null(result);
    }

    [Fact(Skip = _skipReason)]
    public void WhenConnectionStringIsInvalid_ThenExceptionShouldBeThrownDuringStart()
    {
        // Arrange
        var sut = (_serviceCollection
            .Configure<RabbitMQSagaOptions>(x => x.ConnectionString = "amqp://user:pass@host:10000/vhost")
            .BuildServiceProvider()
            .GetRequiredService<IHostedService>() as SagaBackgroundService)!;

        // Act
        var starting = () => sut.StartAsync(_ctsSut.Token);

        // Assert
        Assert.ThrowsAnyAsync<Exception>(starting);
    }

    private string? ExecuteTransactionStep(
        string? transactionId,
        string eventName,
        string nextEventName,
        ResultValidation resultValidation,
        MessageValidation messageValidation,
        bool acknowladge = true)
    {
        var data = new TestData($"{eventName}-Value");
        var compensation = new TestData($"{eventName}-Compensation");
        var inputMessage = new InputMessage(
            transactionId,
            data,
            compensation);

        var messageBytes = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(inputMessage));

        _channel.BasicPublish(
            _exchange,
            eventName,
            _properties,
            messageBytes);

        var result = _gettingRetryPolicy.Execute(() =>
            _channel.BasicGet(nextEventName, acknowladge));

        resultValidation(result);

        if (result is not null && !acknowladge)
        {
            _channel.BasicNack(result.DeliveryTag, false, false);
        }

        var message = result is not null
            ? JsonSerializer.Deserialize<OutputMessage>(
                Encoding.UTF8.GetString(result.Body.Span))
            : null;
        messageValidation(data, message);

        return message?.TransactionId;
    }

    delegate void ResultValidation(BasicGetResult? result);
    delegate void MessageValidation(TestData data, OutputMessage? message);

    private static void ValidResult(BasicGetResult? result)
    {
        Assert.NotNull(result);
    }

    private static void NullResult(BasicGetResult? result)
    {
        Assert.Null(result);
    }

    private static void ValidMessage(TestData data, OutputMessage? message)
    {
        Assert.NotNull(message);
        Assert.NotNull(message?.TransactionId);
        Assert.Equal(data, ((JsonElement)message!.Payload).Deserialize<TestData>());
    }

    private static void NullMessage(TestData data, OutputMessage? message)
    {
        Assert.Null(message);
        Assert.Null(message?.TransactionId);
    }

    public void Dispose()
    {
        _ctsSut.Cancel();
        _sut.Dispose();
        _connection.Dispose();
        _channel.Dispose();
    }
}
