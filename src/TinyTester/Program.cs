using TinyLogger;
using TinyTester;

var l = new TinyLogger<Program>();
var m = new LogMessages(l);

m.HellOWorld();
m.HelloName("World");
