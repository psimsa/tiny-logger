using Microsoft.Extensions.Logging;

namespace TinyLogger;

public class TinyLogger<T>(TinyLoggerConfiguration? configuration = null) : TinyLogger(typeof(T)?.FullName ?? "TinyLogger", configuration), ILogger<T>
{
}