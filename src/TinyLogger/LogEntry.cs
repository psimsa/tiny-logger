namespace TinyLogger;

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string? LogLevel { get; set; }
    public string? Category { get; set; }
    public int EventId { get; set; }
    public string? Message { get; set; }
    public string? Exception { get; set; }

    public Dictionary<string, string?>? State { get; set; }
}
