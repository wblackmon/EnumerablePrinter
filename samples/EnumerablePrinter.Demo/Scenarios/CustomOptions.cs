using EnumerablePrinter.Abstractions;
using EnumerablePrinter.Extensions;
namespace EnumerablePrinter.Demo.Scenarios;

public static class CustomOptionsDemo
{
    public static void Run()
    {
        Console.WriteLine("=== Custom Element Formatting (Manual Preprocessing) ===");

        var names = new[] { "Wayne", "Lucius", "Alfred" };

        // Manually apply custom formatting BEFORE calling Print
        var formattedNames = new List<string>();
        foreach (var n in names)
            formattedNames.Add($"<{n}>");

        formattedNames.Print();
        // Output: ["<Wayne>", "<Lucius>", "<Alfred>"]

        Console.WriteLine();

        Console.WriteLine("=== Custom Dictionary Formatting (Manual Preprocessing) ===");

        var dict = new Dictionary<string, int>
        {
            ["Wayne"] = 1,
            ["Lucius"] = 2
        };

        var formattedDict = new Dictionary<string, string>();
        foreach (var kvp in dict)
            formattedDict[kvp.Key.ToUpper()] = kvp.Value.ToString();

        formattedDict.Print();
        // Output: {"WAYNE": "1", "LUCIUS": "2"}

        Console.WriteLine();

        Console.WriteLine("=== Custom Object Formatting (Manual Wrapper Object) ===");

        var product = new Product
        {
            Id = "1",
            Name = "Keyboard",
            Description = "Mechanical"
        };

        var wrapped = new
        {
            Display = $"Product({product.Name}:{product.Description})"
        };

        wrapped.Print();
        // Output: {Display: "Product(Keyboard:Mechanical)"}

        Console.WriteLine();

        Console.WriteLine("=== PrintOptions Formatting ===");

        var printOptions = new PrintOptions
        {
            Pretty = true,
            IndentSize = 4,
            MaxItems = 2
        };

        var report = new
        {
            Title = "Inventory",
            Items = new[] { "Keyboard", "Mouse", "Monitor" }
        };

        report.Print(options: printOptions);

        Console.WriteLine();

        Console.WriteLine("=== Redirecting Output to a Writer ===");

        using var writer = new StringWriter();
        formattedNames.Print(writer);

        Console.WriteLine(writer.ToString());
        // Output: ["<Wayne>", "<Lucius>", "<Alfred>"]
    }
}

public class Product
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}
