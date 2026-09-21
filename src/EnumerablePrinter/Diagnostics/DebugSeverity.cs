namespace EnumerablePrinter.Diagnostics;

/// <summary>
/// Severity levels used by the debug logging helpers.
/// </summary>
public enum DebugSeverity
{
    /// <summary>
    /// Informational diagnostic message.
    /// </summary>
    Info,

    /// <summary>
    /// Diagnostic message describing a potential problem.
    /// </summary>
    Warning,

    /// <summary>
    /// Diagnostic message describing a failure.
    /// </summary>
    Error,

    /// <summary>
    /// Diagnostic message describing an exception.
    /// </summary>
    Exception
}
