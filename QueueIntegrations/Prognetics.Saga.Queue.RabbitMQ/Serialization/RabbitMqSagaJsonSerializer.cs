using System.Text.Json;
using System.Text;

namespace Prognetics.Saga.Queue.RabbitMQ.Serialization;

public class RabbitMQSagaJsonSerializer : IRabbitMQSagaSerializer
{
    public byte[] Serialize<T>(T outputMessage)
    {
        var serializedMessage = JsonSerializer.Serialize(outputMessage);
        return Encoding.UTF8.GetBytes(serializedMessage);
    }

    public T Deserialize<T>(ReadOnlyMemory<byte> messageBytes)
    {
        var messageString = Encoding.UTF8.GetString(messageBytes.Span);
        return JsonSerializer.Deserialize<T>(messageString)!;
    }
}