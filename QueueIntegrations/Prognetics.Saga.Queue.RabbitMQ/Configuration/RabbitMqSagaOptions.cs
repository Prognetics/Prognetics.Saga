namespace Prognetics.Saga.Queue.RabbitMQ.Configuration;

public class RabbitMQSagaOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/json";
    public string Exchange { get; set; } = string.Empty;
    public string DLXExchange { get; set; } = "saga.dlx";
    public string DLXQueue { get; set; } = "dlx-saga";
    public bool DispatchConsumersAsync { get; set; } = true;

    public static RabbitMQSagaOptions Default { get; } = new();
}