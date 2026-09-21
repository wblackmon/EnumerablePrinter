using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace EnumerablePrinter.Diagnostics;

/// <summary>
/// Queues diagnostic messages for writing from a background worker.
/// </summary>
public static class AsyncDebug
{
    private static readonly BlockingCollection<string> _queue = new();
    private static readonly Thread _worker;

    static AsyncDebug()
    {
        _worker = new Thread(() =>
        {
            foreach (var msg in _queue.GetConsumingEnumerable())
            {
                Trace.WriteLine(msg);
            }
        });

        _worker.IsBackground = true;
        _worker.Start();
    }

    /// <summary>
    /// Queues a message for diagnostic output.
    /// </summary>
    /// <param name="message">The message to write.</param>
    public static void Log(string message)
    {
        _queue.Add(message);
    }

    /// <summary>
    /// Completes the queue and waits for queued messages to be written.
    /// </summary>
    public static void Stop()
    {
        _queue.CompleteAdding();
        _worker.Join();
    }
}
