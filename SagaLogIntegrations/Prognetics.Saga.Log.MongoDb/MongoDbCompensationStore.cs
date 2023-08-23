using MongoDB.Driver;
using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Log.MongoDb;

public class MongoDbCompensationStore : ICompensationStore
{
    private readonly IMongoCollection<CompensationRow> _compensations;

    public MongoDbCompensationStore(
        IMongoClient mongoClient,
        MongoDbSagaLogOptions options)
    {
        _compensations = mongoClient
            .GetDatabase(options.DatabaseName)
            .GetCollection<CompensationRow>(options.CompensationCollectionName);
    }

    public async Task Save(
        CompensationRow compensation,
        CancellationToken cancellationToken = default)
        => await _compensations
            .InsertOneAsync(compensation, cancellationToken: cancellationToken);

    public async Task<IReadOnlyList<CompensationRow>> Get(
        string transactionId,
        CancellationToken cancellationToken = default)
        => await _compensations
            .Find(Builders<CompensationRow>.Filter.Eq(x => x.TransactionId, transactionId))
            .ToListAsync(cancellationToken: cancellationToken);

}
