using BenchmarkDotNet.Running;

using TinyLoggerBenchmark;

var summary = BenchmarkRunner.Run<TinyLoggerBenchmarks>();

internal class MyClass { }