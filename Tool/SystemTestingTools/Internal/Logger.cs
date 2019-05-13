using NLog;

namespace SystemTestingTools
{
    internal class Logger
    {
        internal static void Log(LogEventInfo logEvent, object[] parms)
        {
            var session = MockInstrumentation.GetSession();

            var log = $"{logEvent.Level}: {logEvent.FormattedMessage}";

            if(session == null)
                MockInstrumentation.UnsessionedLogs.Add(log);
            else
                MockInstrumentation.SessionLogs[session].Add(log);
        }
    }
}
