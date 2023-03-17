using NSubstitute;
using Prognetics.Saga.Orchestrator;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.Unit.Tests;
public class RabbitMqSagaHostingServiceTests
{
	[Fact]
	public async Task WhenHostingServiceIsCorrectlyConfigured_ThenShouldConfigureChannelCorrectly()
	{
		// Arrange
		const int consumersCount = 10;
		var channel = Substitute.For<IModel>();
		var channelFactory = Substitute.For<IRabbitMqChannelFactory>();
		channelFactory.Create().Returns(channel);

		var sagaQueue = Substitute.For<ISagaQueue>();

		var consumersFactory = Substitute.For<IRabbitMqSagaConsumersFactory>();
		var subscriber = Substitute.For<ISagaSubscriber>();
		var basicConsumer = Substitute.For<IBasicConsumer>();
		var consumers = Enumerable.Range(0, consumersCount)
			.Select(x => new RabbitMqConsumer
			{
				Queue = $"Queue{x}",
				BasicConsumer = basicConsumer,
			}).ToList();
		consumersFactory.Create(channel, sagaQueue).Returns(consumers);

        var subscriberFactory = Substitute.For<IRabbitMqSagaSubscriberFactory>();
        subscriberFactory.Create(channel).Returns(subscriber);

		var sut = new RabbitMqSagaHostingService(
			channelFactory,
			sagaQueue,
			consumersFactory,
			subscriberFactory);

		var source = new CancellationTokenSource();

		// Act
		var task = sut.Listen(source.Token);

		// Assert
		await Assert.ThrowsAsync<TaskCanceledException>(() => {
			source.Cancel();
			return task;
		});

		channel.Received(consumersCount).BasicConsume(
			basicConsumer,
			Arg.Is<string>(x => consumers.Select(x => x.Queue).Contains(x)),
			false,
			string.Empty,
			false,
			false,
			null);

		sagaQueue.Received().Subscribe(subscriber);
	}
}
