namespace Prognetics.Saga.Core.Model;

public class MongoDbSagaLogOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "sagalog";
    public string CollectionName { get; set; } = "transactionLog";
}