using Prognetics.Saga.Parsers.Core.Abstract;
using Prognetics.Saga.Parsers.Core.Model;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text.Json;

namespace Prognetics.Saga.Parser.Json.Reader
{
    public class JsonTransactionsLedgerReader : ITransactionsLedgerReader
    {
        private readonly ReaderConfiguration _configuration;

        public JsonTransactionsLedgerReader(ReaderConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new NullReferenceException("Configuration for reader not set");
            }

            _configuration = configuration;            
        }

        public ConfigurationSource GetSource() => _configuration.Source;

        public async Task<List<Transaction>> ReadFromFileAsync()
        {
            using var stream = File.OpenRead(_configuration.Uri);
            return await JsonSerializer.DeserializeAsync<List<Transaction>>(stream);
        }

        public async Task<List<Transaction>> ReadFromInternetAsync()
        {
            using var client = new HttpClient();
            var fileStream = await client.GetStreamAsync(_configuration.Uri);
            return await JsonSerializer.DeserializeAsync<List<Transaction>>(fileStream);
        }
    }
}
