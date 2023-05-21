using Prognetics.Saga.Orchestrator.Contract.DTO;

namespace Prognetics.Saga.Orchestrator;
public interface ISagaEngine
{
    Task<(string QueueName, OutputMessage Message)?> Process(
        string queueName, InputMessage inputMessage);

    Task<IReadOnlyDictionary<string, OutputMessage>> Compensate(string transactionId);
}