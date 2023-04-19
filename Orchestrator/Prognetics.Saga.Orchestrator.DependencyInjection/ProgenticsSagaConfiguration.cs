using Microsoft.Extensions.DependencyInjection;

namespace Prognetics.Saga.Orchestrator.DependencyInjection;
public static partial class ProgneticsSagaServiceCollectionExtensions
{
    private class ProgenticsSagaConfiguration : IProgenticsSagaConfiguration
    {
        private readonly IList<Type> _hosts = new List<Type>();
        private readonly IList<Type> _modelSources = new List<Type>();

        public ProgenticsSagaConfiguration(IServiceCollection services)
            => Services = services;

        public IReadOnlyList<Type> Hosts => _hosts.ToList();

        public IReadOnlyList<Type> ModelSources => _modelSources.ToList();

        public IServiceCollection Services { get; }

        public IProgenticsSagaConfiguration UseHost<THost>()
            where THost : ISagaHost
        {
            _hosts.Add(typeof(THost));
            return this;
        }

        public IProgenticsSagaConfiguration UseModelSource<TModelSource>()
            where TModelSource : ISagaModelSource
        {
            _modelSources.Add(typeof(TModelSource));
            return this;
        }
    }
}