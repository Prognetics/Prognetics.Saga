using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.DependencyInjection;
using Prognetics.Saga.Parsers.Core.Model;
using System.Reflection;

namespace Prognetics.Saga.Parsers.DependencyInjection
{
    public static class ParserExtensions
    {
        public static ISagaConfiguration UseParser(this ISagaConfiguration configuration,
            Action<ModelSourceOptions> configureOptions)
        {
            configuration.Services.Configure(configureOptions);
            configuration.Services.AddTransient(x => x.GetRequiredService<IOptions<ModelSourceOptions>>().Value);
                         
            configuration.Services.AddSingleton<IEnumerable<ITransactionLedgerSource>>(provider =>
            {
                var result = new List<ITransactionLedgerSource>();
                var options = provider.GetRequiredService<ModelSourceOptions>();

                foreach (var confirguration in options.Configurations)
                {
                    var type = Type.GetType(confirguration.ParserType, true);
                    ConstructorInfo? ctor = type?.GetConstructor(new[] { typeof(ReaderConfiguration) });
                    var parser = ctor?.Invoke(new object[] { confirguration });

                    if (parser != null)
                    {
                        result.Add((ITransactionLedgerSource)parser);
                    }
                }

                return result;
            });

            return configuration;
        }
    }
}
