namespace EnumerablePrinter.Demo.Scenarios;
using EnumerablePrinter.Demo.Models;
using EnumerablePrinter.Extensions;

public static class ObjectDemo
{
    public static void Run()
    {
        Console.WriteLine("\n--- Object Demo ---");

        var person = new Person
        {
            Name = "Wayne",
            Age = 42,
            Address = new Address
            {
                Street = "123 Main St",
                City = "Mansfield"
            }
        };

        // Expected output:
        // {
        //     Name: "Wayne",
        //     Age: 42,
        //     Address: {
        //         Street: "123 Main St",   
        //         City: "Mansfield"
        //     }
        // }

        person.Print();
    }
}
