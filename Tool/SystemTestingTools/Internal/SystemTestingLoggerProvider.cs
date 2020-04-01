using System;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace SystemTestingTools
{
    internal class SystemTestingLoggerProvider : ILoggerProvider
    {
        private static NonFunctionalLogger nonFunctionalLogger = new NonFunctionalLogger();
        private readonly string[] namespaceToIncludeStart;
        private readonly string[] namespaceToExcludeStart;

        public SystemTestingLoggerProvider(string[] namespaceToIncludeStart = null, string[] namespaceToExcludeStart = null)
        {
            this.namespaceToIncludeStart = namespaceToIncludeStart;
            this.namespaceToExcludeStart = namespaceToExcludeStart;
        }

        public ILogger CreateLogger(string sourceNamespace)
        {
            if(ShouldInclude(sourceNamespace) && !ShouldExclude(sourceNamespace))
                return new FunctionalLogger(sourceNamespace);

            return nonFunctionalLogger;
        }

        private bool ShouldInclude(string sourceNamespace)
        {
            if (namespaceToIncludeStart == null) return true;
            return namespaceToIncludeStart.Any(c => sourceNamespace.StartsWith(c));
        }

        private bool ShouldExclude(string sourceNamespace)
        {
            if (namespaceToExcludeStart == null) return false;
            return namespaceToExcludeStart.Any(c => sourceNamespace.StartsWith(c));
        }

        public void Dispose()
        {
            nonFunctionalLogger = null;
        }
    }

    /// <summary>
    /// class used to assign as a mandatory logger but does not log anything, because the namespace is not one we are interested in
    /// </summary>
    internal class NonFunctionalLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return false;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {

        }
    }

    internal class FunctionalLogger : ILogger
    {
        private readonly string source;

        public FunctionalLogger(string source)
        {
            this.source = source;
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var session = ContextRepo.GetSession();

            var log = new LoggedEvent(logLevel, formatter(state, exception), source);

            if (session == null)
                ContextRepo.UnsessionedLogs.Add(log);
            else
                ContextRepo.SessionLogs[session].Add(log);
        }
    }
}
