using EnumerablePrinter.Extensions;
using EnumerablePrinter.Linq;

namespace EnumerablePrinter.Demo.Scenarios;

public static class SliceDemo
{
    public static void Run()
    {
        Console.WriteLine("\n-- Slice Demo ---");

        var numbers = new[] { 10, 20, 30, 40, 50, 60 };
        var fruits  = new[] { "Apple", "Banana", "Cherry", "Date", "Elderberry" };
        var letters = "abcdef".ToCharArray();

        Console.WriteLine("\nNumbers (start: 1, end: 4):");
        numbers.Slice(start: 1, end: 4).Print();

        Console.WriteLine("\nNumbers (step: 2):");
        numbers.Slice(start: 0, end: numbers.Length, step: 2).Print();

        Console.WriteLine("\nFruits (negative start: -3):");
        fruits.Slice(start: -3).Print();

        Console.WriteLine("\nLetters (slice c-e):");
        letters.Slice(start: 2, end: 5).Print();

        Console.WriteLine("\nLetters (step: 2):");
        letters.Slice(start: 0, end: letters.Length, step: 2).Print();
    }
}
