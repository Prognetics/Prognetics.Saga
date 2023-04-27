namespace Prognetics.Saga.Queue.RabbitMQ.Serialization;

public interface IRabbitMQSagaSerializer
{
    byte[] Serialize<T>(T outputMessage);

    T Deserialize<T>(ReadOnlyMemory<byte> messageBytes);
}