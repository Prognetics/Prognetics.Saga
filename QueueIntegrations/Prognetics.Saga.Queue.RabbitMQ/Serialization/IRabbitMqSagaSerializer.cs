using Prognetics.Saga.Orchestrator;
using System;

namespace Prognetics.Saga.Queue.RabbitMQ.Serialization;

public interface IRabbitMqSagaSerializer
{
    byte[] Serialize(OutputMessage inputMessage);
    InputMessage? Deserialize(ReadOnlyMemory<byte> messageBytes);
}