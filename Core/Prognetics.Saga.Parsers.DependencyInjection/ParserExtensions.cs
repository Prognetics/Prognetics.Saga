using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.DependencyInjection;
using Prognetics.Saga.Parsers.Core.Model;

namespace Prognetics.Saga.Parsers.DependencyInjection
{
    public static class ParserExtensions
    {
        public static ISagaConfiguration UseParser(
            this ISagaConfiguration configuration,
            Action<ModelSourceOptions> configureOptions)
        {
            configuration.Services.Configure(configureOptions);
            configuration.Services.AddTransient(x => x.GetRequiredService<IOptions<ModelSourceOptions>>().Value);
                         
            configuration.Services.AddScoped<IEnumerable<IModelSource>>(provider =>
                provider.GetRequiredService<ModelSourceOptions>().Configurations.Aggregate(
                    new List<IModelSource>(),
                    (sources, c) =>
                    {
                        var parser = c.ParserType is not null
                            ? Type.GetType(c.ParserType, true)
                                ?.GetConstructor(new[] { typeof(ReaderConfiguration) })
                                ?.Invoke(new[] { c })
                            : null;

                        if (parser != null)
                        {
                            sources.Add((IModelSource)parser);
                        }

                        return sources;
                    }).ToList());

            return configuration;
        }
    }
}
