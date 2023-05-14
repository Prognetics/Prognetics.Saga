using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;
using Prognetics.Saga.Parsers.Core.Abstract;
using System.IO;
using System.Text.Json;

namespace Prognetics.Saga.Parser.Json.Reader
{
    public class JsonFromFileTransactionLedgerReader : IModelSource
    {
        public async Task<TransactionsLedger> GetModel(CancellationToken cancellation = default)
        {
            using var stream = File.OpenRead(string.Empty);
            return await JsonSerializer.DeserializeAsync<TransactionsLedger>(stream, (JsonSerializerOptions)null, cancellation);
        }
    }
}
