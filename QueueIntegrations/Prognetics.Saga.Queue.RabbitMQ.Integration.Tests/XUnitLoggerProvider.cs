using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Prognetics.Saga.Queue.RabbitMQ.Integration.Tests;
internal class XUnitLoggerProvider : ILoggerProvider
{
    private readonly Channel<string> _messages;

    public XUnitLoggerProvider(Channel<string> messages)
    {
        _messages = messages;
    }
    public ILogger CreateLogger(string categoryName)
        => new XUnitLogger(categoryName, _messages);

    public void Dispose()
    {
    }

    private class XUnitLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly Channel<string> _messages;

        public XUnitLogger(string categoryName, Channel<string> messages)
        {
            _categoryName = categoryName;
            _messages = messages;
        }
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _messages.Writer.TryWrite($"{logLevel} {_categoryName}: {formatter(state, exception)} {exception?.StackTrace ?? string.Empty}");
        }
    }
}