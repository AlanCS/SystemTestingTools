using Microsoft.Extensions.Logging;

namespace SystemTestingTools
{
    /// <summary>
    /// Each logged event in your application
    /// </summary>
    public class LoggedEvent
    {
        internal LoggedEvent(LogLevel logLevel, string message, string source)
        {
            LogLevel = logLevel;
            Message = message;
            Source = source;
        }
        /// <summary>
        /// The level of the log raised
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// The log message (ie: "Could not contact downstream service x")
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The class (with namespace) that logged it
        /// </summary>
        public string Source { get; set; }

        public override string ToString()
        {
            return $"{LogLevel}: {Message}";
        }
    }
}
