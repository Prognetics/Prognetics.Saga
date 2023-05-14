using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Parser.Json.Reader;
using Prognetics.Saga.Parsers.Core.Abstract;
using Prognetics.Saga.Parsers.Core.Model;

namespace Prognetics.Saga.Parser.Json.Extensios
{
    public static class JsonParserExtensions
    {
        public static void RegisterJsonParsers(this IServiceCollection services,
            Action<ModelSourceOptions> configureOptions)
        {
            services.AddSingleton<JsonFromFileTransactionLedgerReader>();
            services.AddSingleton<JsonFromInternetTransactionLedgerReader>();

            services.AddSingleton<Dictionary<ConfigurationSource, IModelSource>>(provider => new Dictionary<ConfigurationSource, IModelSource>
            {
                { ConfigurationSource.Network, provider.GetRequiredService<JsonFromInternetTransactionLedgerReader>() },
                { ConfigurationSource.File, provider.GetRequiredService<JsonFromFileTransactionLedgerReader>() }
            });           

            services.Configure(configureOptions);
            services.AddTransient(x => x.GetRequiredService<IOptions<ModelSourceOptions>>().Value);

            services.AddTransient<IModelSourceFactory, ModelSourceFactory>();
            services.AddSingleton<IEnumerable<IModelSource>>(provider =>
            {
                var factory = provider.GetRequiredService<IModelSourceFactory>();
                return factory.Build();
            });
        }
    }
}
