using System;
using MetroLog;

namespace TCC.ODBDriver.Services
{
    public class LoggingServices : ILoggingServices
    {
        public static LoggingServices Instance { get; }
        public static int RetainDays { get; } = 3;
        public static bool Enabled { get; set; } = true;

        static LoggingServices()
        {
            Instance = Instance ?? new LoggingServices();
        }

        public void WriteLine<T>(string message, LogLevel logLevel = LogLevel.Trace, Exception exception = null)
        {
            throw new NotImplementedException();
        }
    }
}
