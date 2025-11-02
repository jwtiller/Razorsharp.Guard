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
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var message = $"{timestamp} [{logLevel}] ";

            // if structure ex {@employee}
            if (state is IEnumerable<KeyValuePair<string, object?>> kvps)
            {
                var parts = kvps
                    .Where(kv => kv.Key != "{OriginalFormat}")
                    .Select(kv =>
                    {
                        var value = FormatValue(kv.Value);
                        return $"{kv.Key}={value}";
                    });

                message += string.Join(", ", parts);
            }
            else
            {
                // fallback
                message += formatter(state, exception);
            }

            if (exception != null)
                message += Environment.NewLine + exception;

            lock (_lock)
            {
                File.AppendAllText(_filePath, message + Environment.NewLine);
            }
        }

        private static string FormatValue(object? value)
        {
            if (value == null) return "null";

            var type = value.GetType();

           
            if (!type.IsPrimitive && type != typeof(string))
            {
                var props = type.GetProperties()
                    .Select(p => $"{p.Name}={p.GetValue(value) ?? "null"}");
                return "{" + string.Join(", ", props) + "}";
            }

            return value.ToString() ?? "";
        }

    }
}