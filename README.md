# tiny-logger
A small, performant library for basic console logging in .Net projects. Implements the `ILogger` interface from Microsoft.Extensions.Logging, but with a smaller footprint and list of dependencies.

The library provides basic feature set for logging to the console while being usable in components and dependencies that use ILogger for output, well usable for instance in AOT-published tools.

## Features
- Does not use `Microsoft.Extensions.DependencyInjection` at all (unlike `LoggerFactory.Create` which uses it under the hood)
- Fully supports AOT / code trimming
- Outputs structured logs in JSON format
- Minimal footprint in terms of memory usage, allocations etc.
- Supports log levels, log templates and code-generated log methods
- Can be used in any .Net project, including .Net Standard libraries (from 2.0)
- Can be used in code-generated DI containers (e.g. `Pure.DI`)
- Performs well in container or cloud scenarios where console output is captured by the runtime and further analyzed in e.g. CloudWatch or Application Insights
- Performance comparable with `Microsoft.Extensions.Logging` (see benchmarks project in `test` folder / snapshot below)

## Usage
```csharp
ILogger logger = new TinyLogger<Program>();
logger.LogInformation("Hello world!");
```

## Benchmarks
``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1555/22H2/2022Update/SunValley2)
11th Gen Intel Core i7-11850H 2.50GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.203
  [Host]     : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  Job-MFQIFX : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Runtime=.NET 7.0  IterationCount=1  

```
|                         Method |         Mean | Error | Allocated |
|------------------------------- |-------------:|------:|----------:|
|    Log_SingleThread_TinyLogger |     119.5 μs |    NA |   1.28 KB |
| Log_SingleThread_ConsoleLogger |     114.2 μs |    NA |   1.22 KB |
|     Log_MultiThread_TinyLogger | 112,887.2 μs |    NA | 370.11 KB |
|  Log_MultiThread_ConsoleLogger | 116,492.2 μs |    NA | 666.77 KB |
