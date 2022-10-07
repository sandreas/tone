using Microsoft.Extensions.Logging;

namespace tone.Commands.Settings.Interfaces;

public interface ILoggerSettings
{
    public LogLevel LogLevel { get; set; }
    public string LogFile { get; }
}