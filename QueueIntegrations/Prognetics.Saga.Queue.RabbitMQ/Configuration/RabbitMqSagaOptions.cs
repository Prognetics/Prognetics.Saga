namespace Prognetics.Saga.Queue.RabbitMQ.Configuration;

public class RabbitMqSagaOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/json";
    public string Exchange { get; set; } = string.Empty;
    public bool DispatchConsumersAsync { get; set; } = true;
}