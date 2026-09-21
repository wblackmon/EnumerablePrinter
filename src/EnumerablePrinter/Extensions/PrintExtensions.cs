using EnumerablePrinter.Abstractions;
namespace EnumerablePrinter.Extensions;

/// <summary>
/// Extension methods for writing formatted values to text output.
/// </summary>
public static class PrintExtensions
{
    /// <summary>
    /// Formats a value and writes it to the console followed by a newline.
    /// </summary>
    /// <typeparam name="T">The type of the value being printed.</typeparam>
    /// <param name="value">The value to format and write.</param>
    /// <param name="options">Optional formatting options.</param>
    /// <returns>The original value.</returns>
    public static T PrintToConsole<T>(this T value, PrintOptions? options = null)
    {
        return value.Print(Console.Out, options);
    }

    /// <summary>
    /// Formats a value and writes it to a text writer followed by a newline.
    /// </summary>
    /// <typeparam name="T">The type of the value being printed.</typeparam>
    /// <param name="value">The value to format and write.</param>
    /// <param name="writer">The destination writer, or the console when omitted.</param>
    /// <param name="options">Optional formatting options.</param>
    /// <returns>The original value.</returns>
    public static T Print<T>(this T value, TextWriter? writer = null, PrintOptions? options = null)
    {
        writer ??= Console.Out;
        writer.WriteLine(ObjectFormatter.Format(value, options));
        return value;
    }
}