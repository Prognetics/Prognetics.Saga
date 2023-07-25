using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;
using Prognetics.Saga.Parsers.Core.Model;
using System.Text.Json;

namespace Prognetics.Saga.Parser.Json.Reader
{
    public class JsonFromInternetTransactionLedgerReader : ITransactionLedgerSource
    {
        private readonly ReaderConfiguration _readerConfiguration;

        public JsonFromInternetTransactionLedgerReader(ReaderConfiguration readerConfiguration)
        {
            _readerConfiguration = readerConfiguration;
        }

        public async Task<TransactionsLedger> GetTransactionLedger(CancellationToken cancellation = default)
        {
            using var client = new HttpClient();
            var fileStream = await client.GetStreamAsync(_readerConfiguration.Path);
            return await JsonSerializer.DeserializeAsync<TransactionsLedger>(fileStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true}, cancellation);
        }
    }
}
