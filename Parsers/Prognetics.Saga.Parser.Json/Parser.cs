using Prognetics.Saga.Parsers.Core.Abstract;
using Prognetics.Saga.Parsers.Core.Model;

namespace Prognetics.Saga.Parser.Json
{
    public class Parser : IParser
    {
        private readonly ITransactionsLedgerReader _reader;

        public Parser(ITransactionsLedgerReader reader)
        {
            _reader = reader;
        }

        public async Task<List<Transaction>> GetTransactionsAsync()
        {
            return _reader.GetSource() switch
            {
                ConfigurationSource.File => await _reader.ReadFromFileAsync(),
                ConfigurationSource.Network => await _reader.ReadFromInternetAsync(),
                _ => throw new FileLoadException("File source for reader not set"),
            };
        }
    }
}
