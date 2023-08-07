namespace Prognetics.Saga.Core.Model;

public class MongoDbSagaLogOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "sagalog";
    public string TransactionLogCollectionName { get; set; } = "transactionLogs";
    public string CompensationCollectionName { get; set; } = "compensations";
}