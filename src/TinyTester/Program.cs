using Microsoft.Extensions.Logging;
using TinyLogger;
using TinyTester;

var l = new TinyLogger<Program>();
var m = new LogMessages(l);

m.HellOWorld();
m.HelloName("World");

var sw = System.Diagnostics.Stopwatch.StartNew();

/*for (int i = 0; i < 100_000; i++)
{
    l.LogInformation("Hell-o-world # {Number}!", i);
}*/

sw.Stop();
l.LogInformation("Finished in {Seconds} ms", sw.ElapsedMilliseconds);            