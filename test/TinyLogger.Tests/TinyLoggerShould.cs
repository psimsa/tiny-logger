using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace TinyLogger.Tests;

public class TinyLoggerShould
{
    [Fact]
    public void LogJsonToConsole_GivenValidInput()
    {
        // Arrange
        var logger = new TinyLogger<DummyClass>();
        var message = "Test message";
        var exception = new Exception("Test exception");

        // Redirect console output to a StringWriter
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);
        var originalConsoleOut = Console.Out;
        Console.SetOut(stringWriter);

        // Act
        logger.Log(LogLevel.Error, new EventId(1), message, exception, (s, e) => s);

        // Give the logger enough time to flush the entry
        Task.Delay(1000).Wait();

        // Reset console output to the original value
        Console.SetOut(originalConsoleOut);

        // Assert
        var consoleOutput = stringBuilder.ToString();
        var logEntryLines = consoleOutput.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Single(logEntryLines);

        var logEntry = JsonSerializer.Deserialize<LogEntry>(logEntryLines[0]);
        Assert.NotNull(logEntry);
        Assert.Equal(LogLevel.Error.ToString(), logEntry.LogLevel);
        Assert.Equal(1, logEntry.EventId);
        Assert.Equal(message, logEntry.Message);
        Assert.Equal(exception.ToString(), logEntry.Exception);
    }

    [Fact]
    public void FlushEntriesOnDispose()
    {
        // Arrange
        var logger = new TinyLogger<DummyClass>();

        // Redirect console output to a StringWriter
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);
        var originalConsoleOut = Console.Out;
        Console.SetOut(stringWriter);

        // Act
        for (int i = 0; i < 5; i++)
        {
            logger.Log(LogLevel.Information, new EventId(i), $"Test message {i}", null, (s, e) => s);
        }

        // Reset console output to the original value
        Console.SetOut(originalConsoleOut);

        // Assert
        var consoleOutput = stringBuilder.ToString();
        var logEntryLines = consoleOutput.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(5, logEntryLines.Length);

        var logEntries = logEntryLines.Select(line => JsonSerializer.Deserialize<LogEntry>(line)).ToList();
        for (int i = 0; i < 5; i++)
        {
            Assert.Equal(LogLevel.Information.ToString(), logEntries[i].LogLevel);
            Assert.Equal(i, logEntries[i].EventId);
            Assert.Equal($"Test message {i}", logEntries[i].Message);
            Assert.Null(logEntries[i].Exception);
        }
    }

    private class DummyClass
    {
    }
}