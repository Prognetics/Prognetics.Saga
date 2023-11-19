using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;
using Prognetics.Saga.Parsers.Core.Model;
using System.Text.Json;

namespace Prognetics.Saga.Parser.Json.Reader
{
    public class JsonFromFileTransactionLedgerReader : ITransactionLedgerSource
    {
        private readonly ReaderConfiguration _readerConfiguration;

        public JsonFromFileTransactionLedgerReader(ReaderConfiguration readerConfiguration)
        {
            _readerConfiguration = readerConfiguration;

            if (readerConfiguration.MonitorSource)
            {
                MonitorSourceChanges();
            }
        }

        public async Task<TransactionsLedger> GetTransactionLedger(CancellationToken cancellation = default)
        {
            using var stream = File.OpenRead(_readerConfiguration.Path);
            return await JsonSerializer.DeserializeAsync<TransactionsLedger>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellation);
        }

        private async Task MonitorSourceChanges()
        {
            using var watcher = new FileSystemWatcher(_readerConfiguration.Path);

            watcher.Changed += (s, e) =>
            {                
                ModelChanged?.Invoke(this, e);
            };
        }
    }
}
