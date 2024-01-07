namespace Prognetics.Saga.Queue.RabbitMQ.Configuration;
internal static class RabbitMqConsts
{
    public const string DLX_EXCHANGE_QUEUE_HEADER_NAME = "x-dead-letter-exchange";
    public const string DLX_ROUTING_KEY_QUEUE_HEADER_NAME = "x-dead-letter-routing-key";
}
