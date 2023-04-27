using Prognetics.Saga.Orchestrator.Model;

namespace Prognetics.Saga.Orchestrator.Unit.Tests;
public class SagaOrchestratorFactoryTests
{
    [Theory]
    [InlineData(new int[] { }, 0)]
    [InlineData(new[] { 0 }, 0)]
    [InlineData(new[] { 1 }, 1)]
    [InlineData(new[] { 5 }, 3)]
    [InlineData(new[] { 0, 0, 0 }, 0)]
    [InlineData(new[] { 0, 1, 2 }, 1)]
    [InlineData(new[] { 1, 2, 3 }, 3)]
    public async Task ShouldReturnCorrectNumberOfTransactionsAndStepsFromAllSources(
        int [] transactionsCountPerSource,
        int stepsCountPerTransaction)
    {
        const string fromPrefix = nameof(fromPrefix);
        const string toPrefix = nameof(toPrefix);

        var sources = transactionsCountPerSource.Select((transactionsCount, sourceNumber) =>
            new DelegateSagaModelSource(builder =>
                Enumerable.Range(0, transactionsCount).ToList().ForEach(ti =>
                builder.AddTransaction(transaction =>
                    Enumerable.Range(0, stepsCountPerTransaction).ToList().ForEach(si =>
                    transaction.AddStep(
                            $"{fromPrefix}, source: {sourceNumber}, transaction: {ti}, step: {si}",
                            $"{toPrefix}, source: {sourceNumber}, transaction: {ti}, step: {si}"))))));

        var sut = new SagaOrchestratorFactory(sources);

        var model = (await sut.Create(CancellationToken.None)).Model;
        
        Assert.NotNull(model);

        var allTransactionsCount = transactionsCountPerSource.Sum();
        Assert.Equal(
            allTransactionsCount,
            model.Transactions.Count);

        var allStepsCount = allTransactionsCount * stepsCountPerTransaction;
        Assert.Equal(
            allStepsCount,
            model.Transactions.SelectMany(t => t.Steps).Count());
    }
}
