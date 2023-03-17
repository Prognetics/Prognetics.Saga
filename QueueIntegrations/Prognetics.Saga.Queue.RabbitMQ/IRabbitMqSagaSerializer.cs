using System.Text.Json;
using System.Text;
using Prognetics.Saga.Orchestrator;
using System;

namespace Prognetics.Saga.Queue.RabbitMQ;

public interface IRabbitMqSagaSerializer
{
    byte[] Serialize(OutputMessage inputMessage);
    InputMessage? Deserialize(ReadOnlyMemory<byte> messageBytes);
}

class RabbitMqSagaSerializer : IRabbitMqSagaSerializer
{
    public byte[] Serialize(OutputMessage outputMessage)
    {
        var serializedMessage = JsonSerializer.Serialize(outputMessage);
        return Encoding.UTF8.GetBytes(serializedMessage);
    }

    public InputMessage? Deserialize(ReadOnlyMemory<byte> messageBytes)
    {
        var messageString = Encoding.UTF8.GetString(messageBytes.Span);
        return JsonSerializer.Deserialize<InputMessage>(messageString);
    }

}