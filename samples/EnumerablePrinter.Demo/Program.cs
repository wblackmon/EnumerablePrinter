using EnumerablePrinter.Demo.Scenarios;
using EnumerablePrinter.Extensions;

Console.WriteLine("=== Enumerable Printer Demo ===");
Console.WriteLine();

Console.WriteLine("Console-friendly output:");
new[] { 1, 2, 3 }.PrintToConsole();

Console.WriteLine();
Console.WriteLine("Structured examples:");
SequenceDemo.Run();
DictionaryDemo.Run();
NestedDemo.Run();
ObjectDemo.Run();
CustomOptionsDemo.Run();

Console.WriteLine();
Console.WriteLine("=== End of Demo ===");
