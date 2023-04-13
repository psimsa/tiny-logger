using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.Logging;
using TinyLogger;

namespace TinyLoggerBenchmark;

[MemoryDiagnoser]
//[SimpleJob(RuntimeMoniker.NativeAot70, iterationCount: 1)]
[SimpleJob(RuntimeMoniker.Net70, iterationCount: 1)]
public class TinyLoggerBenchmarks
{
    private TinyLogger<MyClass> _tinyLogger;
    private ILogger<MyClass> _consoleLogger;

    private int tinyCount = 0;
    private int consoleCount = 0;

    [GlobalSetup]
    public void Setup()
    {
        _tinyLogger = new TinyLogger<MyClass>();
        _consoleLogger = LoggerFactory.Create(_ => _.AddJsonConsole()).CreateLogger<MyClass>();
    }

    [Benchmark]
    public void Log_SingleThread_TinyLogger()
    {
        _tinyLogger.LogInformation(new EventId(1), "Test message {Number}", tinyCount);
        tinyCount++;
    }

    [Benchmark]
    public void Log_SingleThread_ConsoleLogger()
    {
        _consoleLogger.LogInformation(new EventId(1), "Test message {Number}", consoleCount);
        consoleCount++;
    }

    [Benchmark]
    public async Task Log_MultiThread_TinyLogger()
    {
        await LogMultipleMessagesAsync(_tinyLogger);
    }

    [Benchmark]
    public async Task Log_MultiThread_ConsoleLogger()
    {
        await LogMultipleMessagesAsync(_consoleLogger);
    }

    private async Task LogMultipleMessagesAsync(ILogger logger)
    {
        var numberOfThreads = 10;
        var logCallsPerThread = 100;

        async Task LogFromThread()
        {
            for (int i = 0; i < logCallsPerThread; i++)
            {
                logger.Log(LogLevel.Information, new EventId(1), "Test message", null, (s, e) => s);
            }
        }

        var tasks = new Task[numberOfThreads];
        for (int i = 0; i < numberOfThreads; i++)
        {
            tasks[i] = Task.Run(LogFromThread);
        }

        await Task.WhenAll(tasks);
    }
}