namespace EnumerablePrinter.Demo.Scenarios;
using EnumerablePrinter.Extensions;

public static class NestedDemo
{
    public static void Run()
    {
        Console.WriteLine("\n-- Nested Demo ---");
        var nestedList = new List<List<int>>
        {
            new List<int> { 1, 2, 3 },
            new List<int> { 4, 5, 6 }
        };

        // Expected output:
        // [[1, 2, 3], [4, 5, 6]]

        nestedList.Print();
    }

}
