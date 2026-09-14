using EnumerablePrinter.Abstractions;
using EnumerablePrinter.Formatters;

namespace EnumerablePrinter.Extensions;

public static class PrintExtensions
{
    private static readonly IObjectFormatter _default = new DefaultObjectFormatter();

    public static T PrintToConsole<T>(this T value)
    {
        return value.Print(Console.Out);
    }

    public static T Print<T>(this T value, TextWriter? writer = null)
    {
        writer ??= Console.Out;
        writer.WriteLine(_default.Format(value));
        return value;
    }
}