using Prognetics.Saga.Parsers.Core.Abstract;
using Prognetics.Saga.Parsers.Core.Model;

namespace Prognetics.Saga.Parser.Json.Reader
{
    public class JsonTransactionsLedgerReader : ITransactionsLedgerReader
    {
        public void Configure(ReaderConfiguration configuration)
        {
            throw new NotImplementedException();
        }

        public List<Transaction> ReadTransactions()
        {
            throw new NotImplementedException();
        }
    }
}
