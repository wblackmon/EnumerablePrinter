namespace EnumerablePrinter.Abstractions;

/// <summary>
/// Formats a value as human-readable diagnostic text.
/// </summary>
public interface IObjectFormatter
{
    /// <summary>
    /// Formats the specified value using the supplied options.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="options">The formatting options to apply.</param>
    /// <returns>The formatted value without a trailing newline.</returns>
    string Format(object? value, PrintOptions options);
}
