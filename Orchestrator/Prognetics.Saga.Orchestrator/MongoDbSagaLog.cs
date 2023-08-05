using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Prognetics.Saga.Orchestrator.Contract;
using Prognetics.Saga.Orchestrator.Contract.DTO;

namespace Prognetics.Saga.Orchestrator;
internal class MongoDbSagaLog : ISagaLog
{
    private const string _transactionStatesCollectionName = "transactionLog";
    private readonly IMongoDatabase _mongoClient;

    public MongoDbSagaLog(IOptions<SagaLogOptions> sagaLogOptions)
    {
        _mongoClient = new MongoClient(sagaLogOptions.Value.ConnectionString)
            .GetDatabase(sagaLogOptions.Value.DatabaseName);
    }

    public async Task<TransactionLog> GetTransactionState(
        string transactionId,
        CancellationToken cancellationToken = default)
        => await _mongoClient
            .GetCollection<TransactionLog>(_transactionStatesCollectionName)
            .AsQueryable()
            .Where(x => x.TransactionId == transactionId)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task SaveTransactionState(
        TransactionLog transactionLog,
        CancellationToken cancellationToken = default)
        => await _mongoClient
            .GetCollection<TransactionLog>(_transactionStatesCollectionName)
            .InsertOneAsync(transactionLog, cancellationToken: cancellationToken);
}

public class SagaLogOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "sagalog";
}