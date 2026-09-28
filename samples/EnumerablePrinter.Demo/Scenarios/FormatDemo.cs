using EnumerablePrinter.Abstractions;
using EnumerablePrinter.Extensions;
using EnumerablePrinter.Linq;

namespace EnumerablePrinter.Demo.Scenarios;

public static class FormatDemo
{
    public static void Run()
    {
        Console.WriteLine("\n-- Format Demo ---");

        var request = new
        {
            Id = 42,
            Path = "/api/orders",
            Tags = new[] { "checkout", "priority", "retry" }
        };

        Console.WriteLine("Formatted request details:");
        var requestText = request.Format(new PrintOptions
        {
            Pretty = true,
            IndentSize = 2,
            MaxDepth = 4,
            MaxItems = 10
        });
        Console.WriteLine(requestText);

        var values = Enumerable.Range(1, 10);
        var sampleText = values
            .Slice(start: 1, end: 9, step: 2)
            .Format();

        Console.WriteLine("Formatted selected sequence:");
        Console.WriteLine(sampleText);
    }
}