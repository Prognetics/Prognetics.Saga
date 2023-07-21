using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;
using Prognetics.Saga.Orchestrator.Contract;
using Prognetics.Saga.Orchestrator.Contract.DTO;

namespace Prognetics.Saga.Orchestrator;

public class SagaOrchestrator : IStartableSagaOrchestrator
{
    private readonly IReadOnlyDictionary<string, string> _steps;
    private ISagaSubscriber? _sagaSubscriber;

    public bool IsStarted { get; private set; }


    public SagaOrchestrator(ITransactionLedgerAccessor transactionLedgerAccessor)
    {
        _steps = GetSteps(transactionLedgerAccessor.TransactionsLedger);
    }

    public void Start(ISagaSubscriber sagaSubscriber)
    {
        _sagaSubscriber = sagaSubscriber;
    }

    public async Task Push(string queueName, InputMessage inputMessage)
    {
        if (!IsStarted || _sagaSubscriber is null)
        {
            throw new InvalidOperationException("Orchestrator not started");
        }

        if (!_steps.TryGetValue(queueName, out var nextStep))
        {
            throw new ArgumentException("Queue name not defined", nameof(queueName));
        }

        var outputMessage = new OutputMessage(
            inputMessage.TransactionId ?? Guid.NewGuid().ToString(),
            inputMessage.Payload);
        
        await _sagaSubscriber.OnMessage(
                nextStep,
                outputMessage);
    }

    private IReadOnlyDictionary<string, string> GetSteps(TransactionsLedger transactionsLedger)
        => transactionsLedger.Transactions
            .SelectMany(x => x.Steps)
            .ToDictionary(
                x => x.EventName,
                x => x.CompletionEventName);

    public void Dispose()
    { }
}