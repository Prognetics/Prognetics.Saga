using Prognetics.Saga.Parsers.Core.Model;

namespace Prognetics.Saga.Parsers.Core.Abstract
{
    public interface ITransactionsLedgerReader
    {
        void Configure(ReaderConfiguration configuration);

        List<Transaction> ReadTransactions();
    }
}
