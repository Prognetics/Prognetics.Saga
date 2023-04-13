using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Prognetics.Saga.Orchestrator;
using Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;
using Prognetics.Saga.Queue.RabbitMQ.Consuming;
using Prognetics.Saga.Queue.RabbitMQ.Hosting;
using Prognetics.Saga.Queue.RabbitMQ.Subscribing;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.Unit.Tests;
public class RabbitMqSagaHostTests
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IRabbitMqConnectionFactory _connectionFactory;
    private readonly IRabbitMqSagaQueuesProvider _sagaQueuesProvider;
    private readonly ISagaOrchestrator _sagaOrchestrator;
    private readonly IRabbitMqSagaConsumersFactory _consumersFactory;
    private readonly ISagaSubscriber _subscriber;
    private readonly IBasicConsumer _basicConsumer;
    private readonly IRabbitMqSagaSubscriberFactory _subscriberFactory;
    private readonly ILogger<IRabbitMqSagaHost> _logger;
    private readonly RabbitMqSagaHost _sut;

    public RabbitMqSagaHostTests()
    {
        _channel = Substitute.For<IModel>();
        _connection = Substitute.For<IConnection>();
        _connectionFactory = Substitute.For<IRabbitMqConnectionFactory>();
        _sagaQueuesProvider = Substitute.For<IRabbitMqSagaQueuesProvider>();
        _sagaOrchestrator = Substitute.For<ISagaOrchestrator>();
        _consumersFactory = Substitute.For<IRabbitMqSagaConsumersFactory>();
        _subscriber = Substitute.For<ISagaSubscriber>();
        _basicConsumer = Substitute.For<IBasicConsumer>();
        _subscriberFactory = Substitute.For<IRabbitMqSagaSubscriberFactory>();
        _logger = Substitute.For<ILogger<IRabbitMqSagaHost>>();

        _connectionFactory.Create().Returns(_connection);
        _connection.CreateModel().Returns(_channel);
        _subscriberFactory.Create(_channel).Returns(_subscriber);

        _sut = new RabbitMqSagaHost(
            _connectionFactory,
            _sagaQueuesProvider,
            _sagaOrchestrator,
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
            .Select(x => new RabbitMqQueue { Name = $"Queue{x}" })
            .ToList();

        _sagaQueuesProvider.Queues.Returns(queues);

        var consumers = Enumerable.Range(0, queuesCount)
			.Select(x => new RabbitMqConsumer
			{
				Queue = queues[x].Name,
				BasicConsumer = _basicConsumer,
			})
            .ToList();

		_consumersFactory.Create(_channel, _sagaOrchestrator).Returns(consumers);
		var source = new CancellationTokenSource();

		// Act
		_sut.Start();

        _channel.Received(queuesCount).QueueDeclare(
            Arg.Is<string>(x => queues.Any(q => q.Name == x)),
            false,
            false,
            false,
            Arg.Is<IDictionary<string, object>>(x => !x.Any()));

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
