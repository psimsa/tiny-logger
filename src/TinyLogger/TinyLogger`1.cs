﻿using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace TinyLogger;

public class TinyLoggerConfiguration
{
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
    public bool IndentedJson { get; set; } = false;
}

public class TinyLogger<T> : ILogger<T>
{
    private readonly string _categoryName;
    private readonly LogEntryJsonContext _jsonContext;
    private readonly LogLevel _logLevel;

    public TinyLogger(TinyLoggerConfiguration? configuration = null)
    {
        configuration ??= new TinyLoggerConfiguration();

        _categoryName = typeof(T)?.FullName ?? "TinyLogger";
        _logLevel = configuration.LogLevel;
        _jsonContext = new LogEntryJsonContext(new JsonSerializerOptions
        {
            WriteIndented = configuration.IndentedJson,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return _logLevel <= logLevel;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        if (formatter == null)
        {
            throw new ArgumentNullException(nameof(formatter));
        }

        var message = formatter(state, exception);
        if (string.IsNullOrEmpty(message) && exception == null)
        {
            return;
        }

        var stateItems = state as IEnumerable<KeyValuePair<string, object>>;

        var logEntry = new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            LogLevel = logLevel.ToString(),
            Category = _categoryName,
            EventId = eventId.Id,
            Message = message,
            Exception = exception?.ToString(),
            State = stateItems?.ToDictionary(x => x.Key, x => x.Value.ToString())
        };

        Console.WriteLine(JsonSerializer.Serialize(logEntry, _jsonContext.LogEntry));
    }
}