using System;
using MetroLog;
using MetroLog.Targets;

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
            
#if DEBUG
            LogManagerFactory.DefaultConfiguration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new StreamingFileTarget { RetainDays = RetainDays});
#else
            LogManagerFactory.DefaultConfiguration.AddTarget(LogLevel.Info, LogLevel.Fatal, new StreamingFileTarget { RetainDays = RetainDays});
#endif
        }

        public void WriteLine<T>(string message, LogLevel logLevel = LogLevel.Trace, Exception exception = null)
        {
            if (!Enabled) return;

            var logger = LogManagerFactory.DefaultLogManager.GetLogger<T>();
            if (logLevel == LogLevel.Trace && logger.IsTraceEnabled)
            {
                logger.Trace(message);
            }
            if (logLevel == LogLevel.Debug && logger.IsDebugEnabled)
            {
                System.Diagnostics.Debug.WriteLine($"{DateTime.Now.TimeOfDay.ToString()} {message}");
            }
            if (logLevel == LogLevel.Error && logger.IsErrorEnabled)
            {
                logger.Error(message);
            }
            if (logLevel == LogLevel.Fatal && logger.IsFatalEnabled)
            {
                logger.Fatal(message);
            }
            if (logLevel == LogLevel.Info && logger.IsInfoEnabled)
            {
                logger.Info(message);
            }
            if (logLevel == LogLevel.Warn && logger.IsWarnEnabled)
            {
                logger.Warn(message);
            }
        }
    }
}
