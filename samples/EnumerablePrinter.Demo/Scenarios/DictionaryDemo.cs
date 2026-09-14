namespace EnumerablePrinter.Demo.Scenarios;
using EnumerablePrinter.Extensions;
public static class DictionaryDemo
{
    public static void Run()
    {
        Console.WriteLine("\n-- Dictionary Demo ---");
        var dict = new Dictionary<string, int>
        {
            { "One", 1 },
            { "Two", 2 },
            { "Three", 3 }
        };

        // Expected output:
        // {"One": 1, "Two": 2, "Three": 3}

        dict.Print();
    }

}
