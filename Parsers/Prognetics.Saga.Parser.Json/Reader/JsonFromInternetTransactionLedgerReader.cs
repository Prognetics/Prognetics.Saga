using Prognetics.Saga.Core.Abstract;
using Prognetics.Saga.Core.Model;
using Prognetics.Saga.Parsers.Core.Model;
using System.Security.Cryptography;
using System.Text.Json;

namespace Prognetics.Saga.Parser.Json.Reader;

public class JsonFromInternetTransactionLedgerReader : ITransactionLedgerSource
{
    private readonly ReaderConfiguration _readerConfiguration;
    private readonly HttpClient _httpClient = new();
    private string? _versionHash;

    public JsonFromInternetTransactionLedgerReader(ReaderConfiguration readerConfiguration)
    {
        _readerConfiguration = readerConfiguration;
    }

    public async Task<TransactionsLedger> GetTransactionLedger(CancellationToken cancellation = default)
    {
        using var fileStream = await _httpClient.GetStreamAsync(
            _readerConfiguration.Path,
            cancellation);
        var result = await JsonSerializer.DeserializeAsync<TransactionsLedger>(
            fileStream,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            },
            cancellation)
            ?? throw new NullReferenceException($"We could not read file from source {nameof(JsonFromInternetTransactionLedgerReader)}");
        _versionHash = Convert.ToBase64String(SHA256.HashData(fileStream));

        return result;
    }

    public async Task TrackTransactionLedger(
        Action<TransactionsLedger> callback,
        Action<Exception> onError,
        CancellationToken cancellation = default)
    {
        if (!_readerConfiguration.TrackingEnabled)
        {
            return;
        }

        while (!cancellation.IsCancellationRequested)
        {
            try
            {
                using var fileStream = await _httpClient.GetStreamAsync(
                    _readerConfiguration.Path,
                    cancellation);

                var currentFileHash = Convert.ToBase64String(SHA256.HashData(fileStream));

                if (currentFileHash != _versionHash)
                {
                    var transactionLedger = await GetTransactionLedger(cancellation);
                    callback(transactionLedger);
                    _versionHash = currentFileHash;
                }
            }
            catch(Exception e)
            {
                onError(e);
            }

            await Task.Delay(_readerConfiguration.TrackingInterval, cancellation);
        }
    }
}
