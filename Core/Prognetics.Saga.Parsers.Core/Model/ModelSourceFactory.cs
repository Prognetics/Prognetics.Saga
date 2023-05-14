using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Parsers.Core.Abstract;

namespace Prognetics.Saga.Parsers.Core.Model
{
    public class ModelSourceFactory : IModelSourceFactory
    {
        private readonly ModelSourceOptions _options;
        private readonly Dictionary<ConfigurationSource, IModelSource> _sources;

        public ModelSourceFactory(ModelSourceOptions options,
            Dictionary<ConfigurationSource, IModelSource> sources)
        {
            _options = options;
            _sources = sources;
        }

        public IEnumerable<IModelSource> Build()
        {
            var result = new List<IModelSource>();

            foreach (var configuration in _options.Configurations)
            {
                result.Add(_sources[configuration.Source]);
            }

            return result;
        }
    }
}
