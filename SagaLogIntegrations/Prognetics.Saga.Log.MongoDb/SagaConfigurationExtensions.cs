using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Prognetics.Saga.Core.DependencyInjection;
using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Log.MongoDb;
public static class SagaConfigurationExtensions
{
    public static ISagaConfiguration UseMongoDbSagaLog(
        this ISagaConfiguration configuration,
        Action<MongoDbSagaLogOptions>? configure = null)
    {
        configuration.Services
            .AddOptions<MongoDbSagaLogOptions>()
            .BindConfiguration("Saga.Log")
            .Configure(configure ?? new Action<MongoDbSagaLogOptions>(_ => { }));

        configuration.Services
            .AddTransient(x => x.GetRequiredService<IOptions<MongoDbSagaLogOptions>>().Value)
            .AddSingleton<IMongoClient>(x => new MongoClient(
                x.GetRequiredService<MongoDbSagaLogOptions>().ConnectionString))
            .AddSingleton<MongoDbCompensationStore>()
            .AddSingleton<MongoDbSagaLog>();

        return configuration;
    }
}
