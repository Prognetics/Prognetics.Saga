using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;
using Prognetics.Saga.Parsers.Core.Model;
using System.Text.Json;

namespace Prognetics.Saga.Parser.Json.Reader;

public class JsonFromFileTransactionLedgerReader : ITransactionLedgerSource
{
    private readonly ReaderConfiguration _readerConfiguration;

    public JsonFromFileTransactionLedgerReader(ReaderConfiguration readerConfiguration)
    {
        _readerConfiguration = readerConfiguration;
    }

    public async Task<TransactionsLedger> GetTransactionLedger(CancellationToken cancellation = default)
    {
        _lastWriteTime = File.GetLastWriteTime(_readerConfiguration.Path);
        using var stream = File.OpenRead(_readerConfiguration.Path);
        return await JsonSerializer.DeserializeAsync<TransactionsLedger>(
            stream,
            new JsonSerializerOptions
            { 
                PropertyNameCaseInsensitive = true 
            }, cancellation) ?? new TransactionsLedger();
    }

    public async Task TrackTransactionLedger(
        Action<TransactionsLedger> callback,
        Action<Exception> onError,
        CancellationToken cancellationToken = default)
    {
        if (!_readerConfiguration.TrackingEnabled)
        {
            return;
        }

        using var watcher = new FileSystemWatcher(_readerConfiguration.Path);
        watcher.EnableRaisingEvents = true;
        watcher.Changed += async (s, e) =>
        {
            try
            {
                var transactionLedger = await GetTransactionLedger(cancellationToken);
                callback(transactionLedger);
            }
            catch (Exception exception)
            {
                onError(exception);
            }
        };

        await Task.FromCanceled(cancellationToken);
    }
}
