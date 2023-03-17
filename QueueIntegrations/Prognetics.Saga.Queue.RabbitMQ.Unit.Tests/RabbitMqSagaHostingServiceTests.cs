using NSubstitute;
using Prognetics.Saga.Orchestrator;
using Prognetics.Saga.Queue.RabbitMQ.ChannelSetup;
using Prognetics.Saga.Queue.RabbitMQ.Consuming;
using Prognetics.Saga.Queue.RabbitMQ.Hosting;
using Prognetics.Saga.Queue.RabbitMQ.Subscribing;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.Unit.Tests;
public class RabbitMqSagaHostingServiceTests
{
    private readonly IModel _channel;
    private readonly IRabbitMqChannelFactory _channelFactory;
    private readonly ISagaQueue _sagaQueue;
    private readonly IRabbitMqSagaConsumersFactory _consumersFactory;
    private readonly ISagaSubscriber _subscriber;
    private readonly IBasicConsumer _basicConsumer;
    private readonly IRabbitMqSagaSubscriberFactory _subscriberFactory;
    private readonly RabbitMqSagaHostingService _sut;

    public RabbitMqSagaHostingServiceTests()
    {
        _channel = Substitute.For<IModel>();
        _channelFactory = Substitute.For<IRabbitMqChannelFactory>();
        _sagaQueue = Substitute.For<ISagaQueue>();
        _consumersFactory = Substitute.For<IRabbitMqSagaConsumersFactory>();
        _subscriber = Substitute.For<ISagaSubscriber>();
        _basicConsumer = Substitute.For<IBasicConsumer>();
        _subscriberFactory = Substitute.For<IRabbitMqSagaSubscriberFactory>();

        _channelFactory.Create().Returns(_channel);
        _subscriberFactory.Create(_channel).Returns(_subscriber);

        _sut = new RabbitMqSagaHostingService(
            _channelFactory,
            _sagaQueue,
            _consumersFactory,
            _subscriberFactory);
    }

	[Fact]
	public async Task WhenHostingServiceIsCorrectlyConfigured_ThenShouldConfigureChannelCorrectly()
	{
		// Arrange
		const int consumersCount = 10;
        var consumers = Enumerable.Range(0, consumersCount)
			.Select(x => new RabbitMqConsumer
			{
				Queue = $"Queue{x}",
				BasicConsumer = _basicConsumer,
			}).ToList();
		_consumersFactory.Create(_channel, _sagaQueue).Returns(consumers);
		var source = new CancellationTokenSource();

		// Act
		var task = _sut.Listen(source.Token);

		// Assert
		await Assert.ThrowsAsync<TaskCanceledException>(() => {
			source.Cancel();
			return task;
		});

		_channel.Received(consumersCount).BasicConsume(
			_basicConsumer,
			Arg.Is<string>(x => consumers.Select(x => x.Queue).Contains(x)),
			false,
			string.Empty,
			false,
			false,
			null);

		_sagaQueue.Received().Subscribe(_subscriber);
	}
}
