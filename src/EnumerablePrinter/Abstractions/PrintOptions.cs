namespace EnumerablePrinter.Abstractions;

/// <summary>
/// Controls how values are formatted by the printer.
/// </summary>
public sealed class PrintOptions
{
    /// <summary>
    /// Maximum nesting depth to format before writing the depth marker.
    /// </summary>
    public int MaxDepth { get; init; } = 50;

    /// <summary>
    /// Enables multiline output with indentation. When false, output is compact.
    /// </summary>
    public bool Pretty { get; init; } = false;

    /// <summary>
    /// Number of spaces to use for each indentation level when <see cref="Pretty"/> is enabled.
    /// </summary>
    public int IndentSize { get; init; } = 2;

    /// <summary>
    /// Includes null-valued properties and collection elements in the output.
    /// </summary>
    public bool IncludeNulls { get; init; } = true;

    /// <summary>
    /// Includes non-public instance fields when formatting objects.
    /// </summary>
    public bool IncludePrivateFields { get; init; } = false;

    /// <summary>
    /// Includes non-public instance properties when formatting objects.
    /// </summary>
    public bool IncludePrivateProperties { get; init; } = false;

    /// <summary>
    /// Maximum number of items to format from each collection.
    /// </summary>
    public int MaxItems { get; init; } = int.MaxValue;

}
