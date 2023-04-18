using Microsoft.Extensions.Logging;
using NSubstitute;
using Prognetics.Saga.Orchestrator;
using Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;
using Prognetics.Saga.Queue.RabbitMQ.Consuming;
using Prognetics.Saga.Queue.RabbitMQ.Hosting;
using Prognetics.Saga.Queue.RabbitMQ.Subscribing;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.Unit.Tests;
public class RabbitMQSagaHostTests
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IRabbitMQConnectionFactory _connectionFactory;
    private readonly IRabbitMQSagaQueuesProvider _sagaQueuesProvider;
    private readonly ISagaOrchestrator _sagaOrchestrator;
    private readonly IRabbitMQSagaConsumersFactory _consumersFactory;
    private readonly ISagaSubscriber _subscriber;
    private readonly IBasicConsumer _basicConsumer;
    private readonly IRabbitMQSagaSubscriberFactory _subscriberFactory;
    private readonly ILogger<IRabbitMQSagaHost> _logger;
    private readonly RabbitMQSagaHost _sut;

    public RabbitMQSagaHostTests()
    {
        _channel = Substitute.For<IModel>();
        _connection = Substitute.For<IConnection>();
        _connectionFactory = Substitute.For<IRabbitMQConnectionFactory>();
        _sagaQueuesProvider = Substitute.For<IRabbitMQSagaQueuesProvider>();
        _sagaOrchestrator = Substitute.For<ISagaOrchestrator>();
        _consumersFactory = Substitute.For<IRabbitMQSagaConsumersFactory>();
        _subscriber = Substitute.For<ISagaSubscriber>();
        _basicConsumer = Substitute.For<IBasicConsumer>();
        _subscriberFactory = Substitute.For<IRabbitMQSagaSubscriberFactory>();
        _logger = Substitute.For<ILogger<IRabbitMQSagaHost>>();

        _connectionFactory.Create().Returns(_connection);
        _connection.CreateModel().Returns(_channel);
        _subscriberFactory.Create(_channel).Returns(_subscriber);

        _sut = new RabbitMQSagaHost(
            _connectionFactory,
            _sagaQueuesProvider,
            _consumersFactory,
            _subscriberFactory,
            _logger);
    }

	[Fact]
	public void WhenHostingIsCorrectlyConfigured_ThenShouldConfigureChannelCorrectly()
	{
		// Arrange
		const int queuesCount = 10;
        var queues = Enumerable.Range(0, queuesCount)
            .Select(x => new RabbitMQQueue { Name = $"Queue{x}" })
            .ToList();

        _sagaQueuesProvider.Queues.Returns(queues);

        var consumers = Enumerable.Range(0, queuesCount)
			.Select(x => new RabbitMQConsumer
			{
				Queue = queues[x].Name,
				BasicConsumer = _basicConsumer,
			})
            .ToList();

		_consumersFactory.Create(_channel, _sagaOrchestrator).Returns(consumers);
		var source = new CancellationTokenSource();

		// Act
		_sut.Start(_sagaOrchestrator);

        _channel.Received(queuesCount).QueueDeclare(
            Arg.Is<string>(x => queues.Any(q => q.Name == x)),
            false,
            false,
            false,
            Arg.Is<IDictionary<string, object>>(x => !x.Any()));

        _channel.DidNotReceiveWithAnyArgs().QueueBind(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IDictionary<string,object>>());

        // Assert
        _channel.Received(queuesCount).BasicConsume(
			_basicConsumer,
			Arg.Is<string>(x => consumers.Select(x => x.Queue).Contains(x)),
			false,
			string.Empty,
			false,
			false,
			null);

		_sagaOrchestrator.Received().Subscribe(_subscriber);
	}

    [Fact]
    public void WhenExchageIsSet_ThenShouldDeclareQueuesCorrectly()
    {
        // Arrange
        const int queuesCount = 10;
        var queues = Enumerable.Range(0, queuesCount)
            .Select(x => new RabbitMQQueue { Name = $"Queue{x}" })
            .ToList();

        const string exchange = "saga";

        _sagaQueuesProvider.Queues.Returns(queues);
        _sagaQueuesProvider.Exchange.Returns(exchange);

        var consumers = Enumerable.Range(0, queuesCount)
            .Select(x => new RabbitMQConsumer
            {
                Queue = queues[x].Name,
                BasicConsumer = _basicConsumer,
            })
            .ToList();

        _consumersFactory.Create(_channel, _sagaOrchestrator).Returns(consumers);
        var source = new CancellationTokenSource();

        // Act
        _sut.Start(_sagaOrchestrator);

        _channel.Received(queuesCount).QueueDeclare(
            Arg.Is<string>(x => queues.Any(q => q.Name == x)),
            false,
            false,
            false,
            Arg.Is<IDictionary<string, object>>(x => !x.Any()));

        _channel.Received(queuesCount).QueueBind(
            Arg.Is<string>(x => queues.Any(q => q.Name == x)),
            Arg.Is(exchange),
            Arg.Is<string>(x => queues.Any(q => q.Name == x)),
            null);


        // Assert
        _channel.Received(queuesCount).BasicConsume(
            _basicConsumer,
            Arg.Is<string>(x => consumers.Select(x => x.Queue).Contains(x)),
            false,
            string.Empty,
            false,
            false,
            null);

        _sagaOrchestrator.Received().Subscribe(_subscriber);
    }
}
