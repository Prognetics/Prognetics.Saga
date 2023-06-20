using NSubstitute;
using Prognetics.Saga.Orchestrator.Contract;
using Prognetics.Saga.Orchestrator.Contract.DTO;

namespace Prognetics.Saga.Orchestrator.Unit.Tests;
public class SagaOrchestratorTests
{
    private readonly ISagaEngine _engine = Substitute.For<ISagaEngine>();
    private readonly ISagaSubscriber _subscriber = Substitute.For<ISagaSubscriber>();
    private readonly SagaOrchestrator _sut;
    private const string _inputEventName = nameof(_inputEventName); 
    private const string _outputEventName = nameof(_outputEventName);
    private const string _transactionId = nameof(_transactionId);
    private static readonly InputMessage _inputMessage = new(
        _transactionId,
        new { Data = "Data" },
        new { Compensation = "Data" });

    private static readonly EngineInput _engineInput = new(
        _inputEventName,
        _inputMessage);

    private static readonly OutputMessage _outputMessage = new(
        _transactionId,
        new { Data = "Data" });

    private static readonly EngineOutput _engineOutput = new(
        _outputEventName,
        _outputMessage);

    public SagaOrchestratorTests()
    {
        _sut = new SagaOrchestrator(_engine);
    }

    [Fact]
    public void IfStartsOrchestratorThenIsStartPropertyShouldBeSetTopTrue()
    {
        // Act
        _sut.Start(_subscriber);

        // Assert
        Assert.True(_sut.IsStarted);
    }

    [Fact]
    public async Task IfOrchestratorIsNotStartedThenExceptionShouldBeThrown()
    {
        // Act
        var push = () => _sut.Push("test", _inputMessage);
        var rollback = () => _sut.Rollback("transactionId");

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(push);
        await Assert.ThrowsAsync<InvalidOperationException>(rollback);
    }

    [Fact]
    public async Task IfEngineRejectsPushedMessageThenSubscriberShouldntBeNotified()
    {
        // Arrange
        _sut.Start(_subscriber);
        _engine.Process(_engineInput).Returns(Task.FromResult((EngineOutput?)null));

        // Act
        await _sut.Push(_inputEventName, _inputMessage);

        // Assert
        await _subscriber
            .DidNotReceive()
            .OnMessage(
                Arg.Any<string>(),
                Arg.Any<OutputMessage>());
    }

    [Fact]
    public async Task IfEngineAcceptsPushedMessageThenSubscriberShouldBeNotified()
    {
        // Arrange
        _sut.Start(_subscriber);
        _engine.Process(_engineInput).Returns(_engineOutput);

        // Act
        await _sut.Push(_inputEventName, _inputMessage);

        // Assert
        await _subscriber
            .Received(1)
            .OnMessage(_outputEventName, _outputMessage);
    }

    [Fact]
    public async Task IfEngineRejectsRollbackThenSubscriberShouldntBeNotified()
    {
        // Arrange
        _sut.Start(_subscriber);
        var output = Task.FromResult(
            Array.Empty<EngineOutput>()
                .AsEnumerable());
        _engine.Compensate(_transactionId)
            .Returns(output);

        // Act
        await _sut.Rollback(_outputEventName);

        // Assert
        await _subscriber
            .DidNotReceive()
            .OnMessage(
                Arg.Any<string>(),
                Arg.Any<OutputMessage>());
    }

    [Fact]
    public async Task IfEngineAcceptsRollbackThenSubscriberShouldBeNotified()
    {
        // Arrange
        const int outputsCount = 10;
        _sut.Start(_subscriber);
        _engine.Compensate(_transactionId)
            .Returns(Enumerable.Repeat(_engineOutput, outputsCount));

        // Act
        await _sut.Rollback(_transactionId);

        // Assert
        await _subscriber
            .Received(outputsCount)
            .OnMessage(_outputEventName, _outputMessage);
    }
}