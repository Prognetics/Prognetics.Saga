﻿using Prognetics.Saga.Core.Model;
using Prognetics.Saga.Orchestrator.Contract.DTO;

namespace Prognetics.Saga.Orchestrator.Contract;

public interface ISagaOrchestrator : IDisposable
{
    TransactionsLedger Model { get; }

    void Subscribe(ISagaSubscriber sagaSubscriber);

    Task Push(string queueName, InputMessage inputMessage);
}