namespace Prognetics.Saga.Parsers.Core.Model
{
    public class ReaderConfiguration
    {
        public string ParserType { get; set; }

        public string Path { get; set; }

        public bool TrackingEnabled { get; set; }
        public TimeSpan TrackingInterval { get; set; } = TimeSpan.FromMinutes(1);
    }
}
