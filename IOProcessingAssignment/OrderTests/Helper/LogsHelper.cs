using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OrderProcessor.Tests.Helper
{

    public class LogsHelper
    {
    }

    public class LogEntry
    {
        public LogLevel LogLevel { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
    }

    public class ListLogger<T> : ILogger<T>
    {
        public List<LogEntry> LogEntries { get; } = new List<LogEntry>();

        public IDisposable BeginScope<TState>(TState state) => default!;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            LogEntries.Add(new LogEntry
            {
                LogLevel = logLevel,
                Message = formatter(state, exception),
                Exception = exception
            });
        }
    }
}
