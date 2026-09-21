using System;
using System.Diagnostics;

namespace EnumerablePrinter.Diagnostics;

/// <summary>
/// Writes filtered diagnostic messages to <see cref="Trace"/>.
/// </summary>
public static class DebugUtility
{
    /// <summary>
    /// Gets or sets the minimum severity that will be logged.
    /// </summary>
    public static DebugSeverity MinimumSeverity { get; set; } = DebugSeverity.Info;

    /// <summary>
    /// Gets or sets the category to log, or null to allow every category.
    /// </summary>
    public static string? CategoryFilter { get; set; } = null;

    /// <summary>
    /// Writes a message when it meets the configured severity and category filters.
    /// </summary>
    /// <param name="message">The message to write.</param>
    /// <param name="severity">The severity of the message.</param>
    /// <param name="category">An optional category used for filtering.</param>
    public static void Log(
        string message,
        DebugSeverity severity = DebugSeverity.Info,
        string? category = null)
    {
        if (severity < MinimumSeverity)
            return;

        if (CategoryFilter != null && category != CategoryFilter)
            return;

        Trace.WriteLine($"[{severity}] {message}");
    }

    /// <summary>
    /// Writes exception type, message, and stack trace details to the diagnostic trace.
    /// </summary>
    /// <param name="ex">The exception to log.</param>
    /// <param name="category">An optional category used for filtering.</param>
    public static void LogException(Exception ex, string? category = null)
    {
        if (CategoryFilter != null && category != CategoryFilter)
            return;

        Trace.WriteLine("[Exception] " + ex.GetType().FullName);
        Trace.WriteLine("[Message] " + ex.Message);
        Trace.WriteLine("[Stack] " + ex.StackTrace);
    }
}
