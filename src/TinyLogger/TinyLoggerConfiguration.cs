using Microsoft.Extensions.Logging;

namespace TinyLogger;

public class TinyLoggerConfiguration
{
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
    public bool IndentedJson { get; set; } = false;
}
