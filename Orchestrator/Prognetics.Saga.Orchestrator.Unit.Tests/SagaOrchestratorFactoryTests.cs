using Prognetics.Saga.Core.Model;

namespace Prognetics.Saga.Orchestrator.Unit.Tests;
public class SagaModelBuilderTests
{
    [Theory]
    [InlineData(new int[] { }, 0)]
    [InlineData(new[] { 0 }, 0)]
    [InlineData(new[] { 1 }, 1)]
    [InlineData(new[] { 5 }, 3)]
    [InlineData(new[] { 0, 0, 0 }, 0)]
    [InlineData(new[] { 0, 1, 2 }, 1)]
    [InlineData(new[] { 1, 2, 3 }, 3)]
    public void ShouldReturnCorrectNumberOfTransactionsAndStepsFromAllSources(
        int [] transactionsCountPerSource,
        int stepsCountPerTransaction)
    {
        const string fromPrefix = nameof(fromPrefix);
        const string toPrefix = nameof(toPrefix);
        const string compensationPrefix = nameof(compensationPrefix);

        var builder = new SagaModelBuilder();
        for (int sourceNumber = 0; sourceNumber < transactionsCountPerSource.Length; sourceNumber++){
            var transactionsCount = transactionsCountPerSource[sourceNumber];
            Enumerable.Range(0, transactionsCount).ToList().ForEach(ti =>
                builder.AddTransaction(
                    $"transaction_{ti}",
                    transaction =>
                        Enumerable.Range(0, stepsCountPerTransaction).ToList().ForEach(si =>
                        transaction.AddStep(
                            $"{fromPrefix}, source: {sourceNumber}, transaction: {ti}, step: {si}",
                            $"{toPrefix}, source: {sourceNumber}, transaction: {ti}, step: {si}",
                            $"{compensationPrefix}, source: {sourceNumber}, transaction: {ti}, step: {si}"))));
        }

        var model = builder.Build();
        
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
