using Microsoft.Extensions.Logging;

namespace TinyTester;

internal partial class LogMessages
{
    private readonly ILogger _logger;

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Hell-o-world!")]
    public partial void HellOWorld();

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Hello {Name}!")]
    public partial void HelloName(string name);

    public LogMessages(ILogger logger)
    {
        _logger = logger;
    }
}