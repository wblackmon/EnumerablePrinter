namespace EnumerablePrinter.Tests;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using EnumerablePrinter.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
[DoNotParallelize]
public class DiagnosticsTests
{
    private StringWriter debugWriter = null!;
    private TextWriterTraceListener listener = null!;

    [TestInitialize]
    public void Setup()
    {
        debugWriter = new StringWriter();
        listener = new TextWriterTraceListener(debugWriter);

        Trace.Listeners.Add(listener);

        // Reset diagnostic settings
        DebugUtility.MinimumSeverity = DebugSeverity.Info;
        DebugUtility.CategoryFilter = null;
    }

    [TestCleanup]
    public void Cleanup()
    {
        Debug.Flush();
        Trace.Listeners.Remove(listener);

        listener.Dispose();
        debugWriter.Dispose();
    }

    private string DebugOutput =>
        debugWriter.ToString().Replace("\r\n", "\n");

    // ---------------------------------------------------------
    // Severity Filtering
    // ---------------------------------------------------------
    [TestMethod]
    public void DebugUtility_RespectsMinimumSeverity()
    {
        DebugUtility.MinimumSeverity = DebugSeverity.Warning;

        DebugUtility.Log("Info message", DebugSeverity.Info);
        DebugUtility.Log("Warning message", DebugSeverity.Warning);

        Debug.Flush();

        Assert.DoesNotContain("Info message", DebugOutput);
        Assert.Contains("[Warning] Warning message", DebugOutput);
    }

    // ---------------------------------------------------------
    // Category Filtering
    // ---------------------------------------------------------
    [TestMethod]
    public void DebugUtility_RespectsCategoryFilter()
    {
        DebugUtility.CategoryFilter = "Target";

        DebugUtility.Log("Ignored message", DebugSeverity.Info, "Other");
        DebugUtility.Log("Included message", DebugSeverity.Info, "Target");

        Debug.Flush();

        Assert.DoesNotContain("Ignored message", DebugOutput);
        Assert.Contains("[Info] Included message", DebugOutput);
    }

    // ---------------------------------------------------------
    // Exception Logging
    // ---------------------------------------------------------
    [TestMethod]
    public void DebugUtility_LogsExceptionDetails()
    {
        var ex = new InvalidOperationException("Something went wrong");

        DebugUtility.LogException(ex);

        Debug.Flush();

        Assert.Contains("[Exception] System.InvalidOperationException", DebugOutput);
        Assert.Contains("[Message] Something went wrong", DebugOutput);
        Assert.Contains("[Stack]", DebugOutput);
    }

    // ---------------------------------------------------------
    // Basic Logging
    // ---------------------------------------------------------
    [TestMethod]
    public void DebugUtility_LogsSimpleMessage()
    {
        DebugUtility.Log("Hello diagnostics!", DebugSeverity.Info);

        Debug.Flush();

        Assert.Contains("[Info] Hello diagnostics!", DebugOutput);
    }

    // ---------------------------------------------------------
    // Async Logging
    // ---------------------------------------------------------
    [TestMethod]
    public void AsyncDebug_LogsMessagesInBackgroundThread()
    {
        AsyncDebug.Log("Async message 1");
        AsyncDebug.Log("Async message 2");

        // Allow background thread to flush
        Thread.Sleep(50);

        AsyncDebug.Stop();
        Debug.Flush();

        Assert.Contains("Async message 1", DebugOutput);
        Assert.Contains("Async message 2", DebugOutput);
    }
}
