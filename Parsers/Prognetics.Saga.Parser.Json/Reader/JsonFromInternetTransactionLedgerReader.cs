using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;
using Prognetics.Saga.Parsers.Core.Model;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Unicode;

namespace Prognetics.Saga.Parser.Json.Reader
{
    public class JsonFromInternetTransactionLedgerReader : IModelSource
    {
        private readonly ReaderConfiguration _readerConfiguration;
        private static string VersionHash;

        public JsonFromInternetTransactionLedgerReader(ReaderConfiguration readerConfiguration)
        {
            _readerConfiguration = readerConfiguration;

            if (readerConfiguration.MonitorSource)
            {
                MonitorSourceChanges();
            }
        }        

        public event EventHandler ModelChanged;

        public async Task<TransactionsLedger> GetModel(CancellationToken cancellation = default)
        {
            using var client = new HttpClient();
            var fileStream = await client.GetStreamAsync(_readerConfiguration.Path);
            var result = await JsonSerializer.DeserializeAsync<TransactionsLedger>(fileStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellation);
            
            if(result == null)
            {
                throw new NullReferenceException($"We could not read file from source {nameof(JsonFromInternetTransactionLedgerReader)}");
            }
            
            VersionHash = Convert.ToBase64String(SHA256.HashData(fileStream));

            return result;
        }

        private async Task MonitorSourceChanges()
        {
            // we need a service here to download the file in periods of time?
            // and check hash value?

            var data = await GetModel(CancellationToken.None);
            var currentFileHash = Convert.ToBase64String(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(data)));

            if(currentFileHash != VersionHash)
            {
                ModelChanged?.Invoke(this, new EventArgs());
            }
            
        }
    }
}
