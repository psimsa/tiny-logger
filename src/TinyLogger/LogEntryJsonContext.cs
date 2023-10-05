using System.Text.Json.Serialization;

namespace TinyLogger;

[JsonSerializable(typeof(LogEntry))]
public partial class LogEntryJsonContext : JsonSerializerContext
{
}
