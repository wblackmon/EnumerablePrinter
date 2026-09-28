using EnumerablePrinter.Abstractions;

namespace EnumerablePrinter.Extensions;

/// <summary>
/// Provides diagnostic formatting extensions for values.
/// </summary>
public static class FormatExtensions
{
    /// <summary>
    /// Formats a value as readable diagnostic text without writing it to an output destination.
    /// </summary>
    /// <typeparam name="T">The compile-time type of the value being formatted.</typeparam>
    /// <param name="value">The value to format.</param>
    /// <param name="options">Optional settings controlling layout, included members, and output limits.</param>
    /// <returns>The diagnostic representation of <paramref name="value"/>.</returns>
    /// <remarks>
    /// This method delegates to <see cref="ObjectFormatter.Format"/> and uses the same
    /// formatting rules as <see cref="PrintExtensions.Print{T}(T, TextWriter, PrintOptions)"/>.
    /// It does not write to the console, a logger, or any other destination.
    ///
    /// Formatting is immediate. If <paramref name="value"/> is enumerable, it is enumerated
    /// during this call. Use <see cref="PrintOptions.MaxItems"/> to limit collection output.
    /// The returned diagnostic text is not JSON serialization or HTML-encoded output.
    /// </remarks>
    /// <example>
    /// <code>
    /// var text = request.Format(new PrintOptions
    /// {
    ///     Pretty = true,
    ///     MaxDepth = 4,
    ///     MaxItems = 50
    /// });
    /// </code>
    /// </example>
    public static string Format<T>(this T value, PrintOptions? options = null)
    {
        return ObjectFormatter.Format(value, options);
    }
}
