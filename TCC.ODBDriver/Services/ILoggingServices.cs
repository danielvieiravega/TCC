using System;
using MetroLog;

namespace TCC.ODBDriver.Services
{
    public interface ILoggingServices
    {
        void WriteLine<T>(string message, LogLevel logLevel = LogLevel.Trace, Exception exception = null);
    }
}
