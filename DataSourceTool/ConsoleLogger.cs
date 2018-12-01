using System;
using Microsoft.Extensions.Logging;

namespace DataSourceTool
{
    public class ConsoleLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel == LogLevel.Warning)
                Console.ForegroundColor = ConsoleColor.Yellow;
            else if (logLevel == LogLevel.Error || logLevel == LogLevel.Critical)
                Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine($"[{logLevel}] {formatter(state, exception)}");

            Console.ResetColor();
        }
    }
}
