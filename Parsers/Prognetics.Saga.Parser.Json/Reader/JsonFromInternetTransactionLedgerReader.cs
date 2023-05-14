using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;
using System.Text.Json;

namespace Prognetics.Saga.Parser.Json.Reader
{
    public class JsonFromInternetTransactionLedgerReader : IModelSource
    {
        public async Task<TransactionsLedger> GetModel(CancellationToken cancellation = default)
        {
            using var client = new HttpClient();
            var fileStream = await client.GetStreamAsync(string.Empty);
            return await JsonSerializer.DeserializeAsync<TransactionsLedger>(fileStream, (JsonSerializerOptions)null, cancellation);
        }
    }
}
