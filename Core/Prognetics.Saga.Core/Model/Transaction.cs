namespace Prognetics.Saga.Core.Model;

public class Transaction    
{
    public string Name { get; init; } = string.Empty;

    public IReadOnlyList<Step> Steps { get; init; } = new List<Step>();
}
