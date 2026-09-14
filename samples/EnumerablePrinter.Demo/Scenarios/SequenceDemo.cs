using EnumerablePrinter.Extensions;
namespace EnumerablePrinter.Demo.Scenarios;

public static class SequenceDemo
{
    public static void Run()
    {
        Console.WriteLine("\n-- Sequence Demo ---");
        var numbers = new[] { 1, 2, 3, 4, 5 };
        var fruits = new[] { "Apple", "Banana", "Cherry" };

        // Expected output:
        // [1, 2, 3, 4, 5]
        // ["apple", "banana", "cherry"]

        Console.WriteLine("Numbers:");
        numbers.Print();

        Console.WriteLine("\nFruits:");
        fruits.Print();
    }
}
