using Microsoft.Extensions.Logging;
using System.IO;

public class FileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;
    private readonly object _lock = new();

    public FileLoggerProvider(string filePath)
    {
        _filePath = filePath;
    }

    public ILogger CreateLogger(string categoryName) => new FileLogger(_filePath, _lock);

    public void Dispose() { }

    private class FileLogger : ILogger
    {
        private readonly string _filePath;
        private readonly object _lock;

        public FileLogger(string filePath, object @lock)
        {
            _filePath = filePath;
            _lock = @lock;
        }

        public IDisposable BeginScope<TState>(TState state) => null!;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {formatter(state, exception)}";
            lock (_lock)
            {
                File.AppendAllText(_filePath, message + Environment.NewLine);
            }
        }
    }
}