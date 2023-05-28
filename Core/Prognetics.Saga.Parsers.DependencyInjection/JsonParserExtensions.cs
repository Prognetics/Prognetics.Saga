using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Parsers.Core.Model;
using System.Reflection;

namespace Prognetics.Saga.Parsers.DependencyInjection
{
    public static class JsonParserExtensions
    {
        public static void RegisterJsonParsers(this IServiceCollection services,
            Action<ModelSourceOptions> configureOptions)
        {
            services.Configure(configureOptions);
            services.AddTransient(x => x.GetRequiredService<IOptions<ModelSourceOptions>>().Value);

            services.AddSingleton<IEnumerable<IModelSource>>(provider =>
            {
                var result = new List<IModelSource>();
                var options = provider.GetRequiredService<ModelSourceOptions>();

                foreach (var confirguration in options.Configurations)
                {
                    var type = Type.GetType(confirguration.ParserType, true);
                    ConstructorInfo ctor = type.GetConstructor(new[] { typeof(ReaderConfiguration) });
                    var parser = ctor.Invoke(new object[] { confirguration });
                    result.Add((IModelSource)parser);
                }

                return result;
            });
        }
    }
}
