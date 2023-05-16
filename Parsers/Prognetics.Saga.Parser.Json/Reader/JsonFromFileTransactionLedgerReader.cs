using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;
using Prognetics.Saga.Parsers.Core.Model;
using System.Text.Json;

namespace Prognetics.Saga.Parser.Json.Reader
{
    public class JsonFromFileTransactionLedgerReader : IModelSource
    {
        private readonly ReaderConfiguration _readerConfiguration;

        public JsonFromFileTransactionLedgerReader(ReaderConfiguration readerConfiguration)
        {
            _readerConfiguration = readerConfiguration;
        }

        public async Task<TransactionsLedger> GetModel(CancellationToken cancellation = default)
        {
            using var stream = File.OpenRead(_readerConfiguration.Uri);
            return await JsonSerializer.DeserializeAsync<TransactionsLedger>(stream, (JsonSerializerOptions)null, cancellation);
        }
    }
}
