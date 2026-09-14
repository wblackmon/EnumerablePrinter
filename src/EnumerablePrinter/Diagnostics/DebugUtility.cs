using System;
using System.Diagnostics;

namespace EnumerablePrinter.Diagnostics;

public static class DebugUtility
{
    public static DebugSeverity MinimumSeverity { get; set; } = DebugSeverity.Info;
    public static string? CategoryFilter { get; set; } = null;

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

    public static void LogException(Exception ex, string? category = null)
    {
        if (CategoryFilter != null && category != CategoryFilter)
            return;

        Trace.WriteLine("[Exception] " + ex.GetType().FullName);
        Trace.WriteLine("[Message] " + ex.Message);
        Trace.WriteLine("[Stack] " + ex.StackTrace);
    }
}
