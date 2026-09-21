using EnumerablePrinter.Abstractions;
using EnumerablePrinter.Formatters;

namespace EnumerablePrinter;

/// <summary>
/// Formats objects, collections, and dictionaries as diagnostic text.
/// </summary>
public static class ObjectFormatter
{
    private static readonly IObjectFormatter Default = new DefaultObjectFormatter();

    /// <summary>
    /// Formats a value without appending a newline.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="options">Optional formatting options.</param>
    /// <returns>The formatted value.</returns>
    public static string Format(object? value, PrintOptions? options = null)
    {
        return Default.Format(value, options ?? new PrintOptions());
    }

    /// <summary>
    /// Writes a formatted value to a text writer without appending a newline.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="writer">The destination writer.</param>
    /// <param name="options">Optional formatting options.</param>
    public static void Write(object? value, TextWriter writer, PrintOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.Write(Default.Format(value, options ?? new PrintOptions()));
    }
}