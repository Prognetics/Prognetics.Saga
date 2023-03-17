namespace Prognetics.Saga.Queue.RabbitMQ;

public class RabbitMqSagaOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/json";
    public string Exchange { get; set; } = string.Empty;
    public bool DispatchConsumersAsync { get; set; } = true;
}