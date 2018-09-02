using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;

namespace MHArmory.Logging
{
    public class DispatcherLogger : ILogger
    {
        private readonly Dispatcher dispatcher;
        private readonly ILogger targetLogger;

        public DispatcherLogger(ILogger targetLogger)
            : this(Dispatcher.CurrentDispatcher, targetLogger)
        {
        }

        public DispatcherLogger(Dispatcher dispatcher, ILogger targetLogger)
        {
            this.dispatcher = dispatcher;
            this.targetLogger = targetLogger;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            if (targetLogger != null)
                return dispatcher.Invoke(() => targetLogger.BeginScope(state));
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            if (targetLogger != null)
                return dispatcher.Invoke(() => targetLogger.IsEnabled(logLevel));
            return false;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (targetLogger != null)
                dispatcher.Invoke(() => targetLogger.Log(logLevel, eventId, state, exception, formatter));
        }
    }
}
