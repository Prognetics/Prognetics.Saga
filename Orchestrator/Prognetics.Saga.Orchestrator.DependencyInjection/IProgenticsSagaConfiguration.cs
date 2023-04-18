﻿namespace Prognetics.Saga.Orchestrator.DependencyInjection;

public interface IProgenticsSagaConfiguration
{
    IProgenticsSagaConfiguration UseHost<THost>()
        where THost : ISagaHost;
    IProgenticsSagaConfiguration UseModelSource<TModelSource>()
        where TModelSource : ISagaModelSource;
}

