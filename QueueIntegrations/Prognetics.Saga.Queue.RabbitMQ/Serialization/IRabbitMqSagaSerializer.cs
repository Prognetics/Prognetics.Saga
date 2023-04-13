using Prognetics.Saga.Orchestrator;
using System;
using System.Text.Json;
using System.Text;

namespace Prognetics.Saga.Queue.RabbitMQ.Serialization;

public interface IRabbitMqSagaSerializer
{
    byte[] Serialize<T>(T outputMessage);

    T Deserialize<T>(ReadOnlyMemory<byte> messageBytes);
}