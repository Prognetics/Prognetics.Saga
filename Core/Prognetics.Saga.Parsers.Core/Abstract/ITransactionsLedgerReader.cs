using Prognetics.Saga.Parsers.Core.Model;

namespace Prognetics.Saga.Parsers.Core.Abstract
{
    public interface ITransactionsLedgerReader
    {
        ConfigurationSource GetSource();

        Task<List<Transaction>> ReadFromFileAsync();

        Task<List<Transaction>> ReadFromInternetAsync();
    }
}
