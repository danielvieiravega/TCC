namespace TCC.ODBDriver
{
    /// <summary>
    /// List of supported PIDs
    /// <see cref="http://en.wikipedia.org/wiki/OBD-II_PIDs"/>
    /// </summary>
    public enum PID
    {
        RPM = 0x0C,
        Speed = 0x0D,
        EngineTemperature = 0x05,
    };
}
