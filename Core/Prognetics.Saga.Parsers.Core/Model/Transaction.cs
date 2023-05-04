namespace Prognetics.Saga.Parsers.Core.Model
{
    public class Transaction
    {
        public string Name { get; set; }

        public List<Operation> Operations { get; set; }
    }
}
