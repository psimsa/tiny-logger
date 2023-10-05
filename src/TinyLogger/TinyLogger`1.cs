using Microsoft.Extensions.Logging;

namespace TinyLogger;

public class TinyLogger<T>: TinyLogger, ILogger<T>
{
    public TinyLogger(TinyLoggerConfiguration? configuration = null) : base(typeof(T)?.FullName ?? "TinyLogger", configuration)
    {        
    }
}