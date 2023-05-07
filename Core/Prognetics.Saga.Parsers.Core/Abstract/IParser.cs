using Prognetics.Saga.Parsers.Core.Model;

namespace Prognetics.Saga.Parsers.Core.Abstract
{
    public interface IParser
    {
        Task<List<Transaction>> GetTransactionsAsync();
    }
}
