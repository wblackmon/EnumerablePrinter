using EnumerablePrinter.Extensions;

namespace EnumerablePrinter.Demo.Scenarios;

public static class ScalarDemo
{
    public static void Run()
    {
        Console.WriteLine("=== Scalar Values ===");
        Console.WriteLine();

        Console.WriteLine("Integral types:");

        ((sbyte)-8).Print();
        ((byte)8).Print();
        ((short)-16).Print();
        ((ushort)16).Print();
        (-32).Print();
        32U.Print();
        (-64L).Print();
        64UL.Print();

        Console.WriteLine();

        Console.WriteLine("Floating-point and decimal types:");

        ((Half)1.5f).Print();
        3.14f.Print();
        3.14159d.Print();
        5.0m.Print();

        Console.WriteLine();

        Console.WriteLine("Boolean values:");

        true.Print();
        false.Print();

        Console.WriteLine();

        Console.WriteLine("Text values:");

        "Hello, EnumerablePrinter!".Print();
        'A'.Print();
        "Line one\nLine two".Print();

        Console.WriteLine();

        Console.WriteLine("Enum:");

        DayOfWeek.Monday.Print();

        Console.WriteLine();

        Console.WriteLine("Date and time values:");

        new DateTime(2024, 1, 1, 12, 30, 0).Print();

        new DateTimeOffset(
            2024,
            1,
            1,
            12,
            30,
            0,
            TimeSpan.Zero).Print();

        new DateOnly(2024, 1, 1).Print();
        new TimeOnly(12, 30, 0).Print();
        new TimeSpan(1, 2, 3, 4).Print();

        Console.WriteLine();

        Console.WriteLine("Guid:");

        Guid.Parse(
            "12345678-1234-1234-1234-123456789abc")
            .Print();

        Console.WriteLine();

        Console.WriteLine("Mixed scalar collection:");

        object?[] values =
        [
            42,
            5.0m,
            3.14d,
            true,
            false,
            null,
            "hello",
            'A',
            DayOfWeek.Monday
        ];

        values.Print();

        Console.WriteLine();
    }
}