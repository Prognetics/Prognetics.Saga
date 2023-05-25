using Microsoft.Extensions.DependencyInjection;

namespace Prognetics.Saga.Core.DependencyInjection;

public interface ISagaConfiguration
{
    IServiceCollection Services { get; }
}