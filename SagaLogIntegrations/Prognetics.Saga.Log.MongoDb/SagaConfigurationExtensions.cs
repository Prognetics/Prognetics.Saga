using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.DependencyInjection;
using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Log.MongoDb;
public static class SagaConfigurationExtensions
{
    public static ISagaConfiguration UseMongoDbSagaLog(
        this ISagaConfiguration configuration,
        Action<MongoDbSagaLogOptions>? configure = null)
    {
        RegisterClassMaps();

        configuration.Services
            .AddOptions<MongoDbSagaLogOptions>()
            .BindConfiguration("Saga.Log")
            .Configure(configure ?? new Action<MongoDbSagaLogOptions>(_ => { }));

        configuration.Services
            .AddTransient(x => x.GetRequiredService<IOptions<MongoDbSagaLogOptions>>().Value)
            .AddSingleton<IMongoClient>(x => new MongoClient(
                x.GetRequiredService<MongoDbSagaLogOptions>().ConnectionString))
            .AddSingleton<ICompensationStore, MongoDbCompensationStore>()
            .AddSingleton<ISagaLog, MongoDbSagaLog>();

        return configuration;
    }

    private static void RegisterClassMaps()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(TransactionLog)))
        {
            BsonClassMap.RegisterClassMap<TransactionLog>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(x => x.TransactionId);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(CompensationRow)))
        {
            BsonClassMap.RegisterClassMap<CompensationRow>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(x => x.Key);
            });
        }
    }
}
