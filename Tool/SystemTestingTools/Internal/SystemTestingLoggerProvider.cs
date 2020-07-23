using System;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace SystemTestingTools
{
    internal class SystemTestingLoggerProvider : ILoggerProvider
    {
        private static NonFunctionalLogger nonFunctionalLogger = new NonFunctionalLogger();
        private readonly LogLevel minimumLevelToIntercept;
        private readonly string[] namespaceToIncludeStart;
        private readonly string[] namespaceToExcludeStart;

        public SystemTestingLoggerProvider(LogLevel minimumLevelToIntercept, string[] namespaceToIncludeStart = null, string[] namespaceToExcludeStart = null)
        {
            this.minimumLevelToIntercept = minimumLevelToIntercept;
            this.namespaceToIncludeStart = namespaceToIncludeStart;
            this.namespaceToExcludeStart = namespaceToExcludeStart;
        }

        public ILogger CreateLogger(string sourceNamespace)
        {
            if(ShouldInclude(sourceNamespace) && !ShouldExclude(sourceNamespace))
                return new FunctionalLogger(minimumLevelToIntercept, sourceNamespace);

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
        private readonly LogLevel minimumLevelToIntercept;
        private readonly string source;

        public FunctionalLogger(LogLevel minimumLevelToIntercept, string source)
        {
            this.minimumLevelToIntercept = minimumLevelToIntercept;
            this.source = source;
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return !IsLogDisabled(logLevel);
        }

        private bool IsLogDisabled(LogLevel logLevel)
        {
            return minimumLevelToIntercept > logLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if(IsLogDisabled(logLevel)) return; // for some reasons, some logs are getting here when they shouldn't; so we do one last check here

            var session = ContextRepo.GetSession();

            var log = new LoggedEvent(logLevel, formatter(state, exception), source);

            if (session == null)
                ContextRepo.UnsessionedLogs.Add(log);
            else
                ContextRepo.SessionLogs[session].Add(log);
        }
    }
}
