using NSubstitute;
using Prognetics.Saga.Orchestrator;
using RabbitMQ.Client;

namespace Prognetics.Saga.Queue.RabbitMQ.Unit.Tests;
public class RabbitMqSagaBackgroundServiceTests
{
	[Fact]
	public async Task Test()
	{
		const int stepsCount = 3;
		var channel = Substitute.For<IModel>();
		var connection = Substitute.For<IConnection>();
		connection.CreateModel().Returns(channel);
		var connectionFactory = Substitute.For<IRabbitMqConnectionFactory>();
		connectionFactory.Create().Returns(connection);
        var sagaModel = Substitute.For<ISagaModel>();
		var transactions = new List<SagaTransaction>
		{
			new SagaTransaction
			{
				Steps = Enumerable.Range(0, stepsCount).Select(i => 
					new SagaTransactionStep
					{
						From = $"From{i}", To = $"To{i}"
					}).ToList()
			},
            new SagaTransaction
            {
                Steps = Enumerable.Range(0, stepsCount).Select(i =>
                    new SagaTransactionStep
                    {
                        From = $"FromAnother{i}", To = $"ToAnother{i}"
                    }).ToList()
            },
        };

		sagaModel.Transactions.Returns(transactions);
		var sagaQueue = Substitute.For<ISagaQueue>();

        var sut = new RabbitMqSagaBackgroundService(connectionFactory, sagaModel, sagaQueue);
		var source = new CancellationTokenSource();
		var task = sut.Listen(source.Token);
		source.Cancel();
		try
		{
			await task;
		}
		catch (TaskCanceledException) { };

		connectionFactory.Received().Create();
		connection.Received().CreateModel();
		channel.Received(stepsCount * transactions.Count).QueueDeclare(queue: Arg.Is<string>(x => x.Contains("From")));
		channel.Received(stepsCount * transactions.Count).QueueDeclare(queue: Arg.Is<string>(x => x.Contains("To")));
		channel.Received(stepsCount * transactions.Count).BasicConsume(Arg.Is<string>(x => x.Contains("From")), true, Arg.Any<IBasicConsumer>());
		sagaQueue.Received().Subscribe(Arg.Any<Func<OutputMessage, Task>>());
    }
}
