using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Orchestrator.SagaLog;
public class MongoDbSagaLog : ISagaLog
{
    private readonly IMongoCollection<TransactionLog> _transactionLogs;

    public MongoDbSagaLog(
        IMongoClient mongoClient,
        MongoDbSagaLogOptions options)
    {
        _transactionLogs = mongoClient
            .GetDatabase(options.DatabaseName)
            .GetCollection<TransactionLog>(options.TransactionLogCollectionName);
    }

    public async Task AddTransaction(
        TransactionLog transactionLog,
        CancellationToken cancellationToken = default)
        => await _transactionLogs
            .InsertOneAsync(transactionLog, cancellationToken: cancellationToken);

    public async Task<TransactionLog> GetTransaction(
        string transactionId,
        CancellationToken cancellationToken = default)
        => await _transactionLogs
            .Find(Builders<TransactionLog>.Filter.Eq(x => x.TransactionId, transactionId))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task UpdateTransaction(
        TransactionLog transactionLog,
        CancellationToken cancellationToken = default)
        => await _transactionLogs
            .FindOneAndUpdateAsync(
                Builders<TransactionLog>.Filter
                    .Eq(x => x.TransactionId, transactionLog.TransactionId),
                Builders<TransactionLog>.Update
                    .Set(x => x.LastCompletionEvent, transactionLog.LastCompletionEvent)
                    .Set(x => x.State, transactionLog.State),
                cancellationToken: cancellationToken);
}