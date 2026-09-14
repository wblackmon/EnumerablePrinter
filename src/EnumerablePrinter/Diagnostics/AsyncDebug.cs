using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace EnumerablePrinter.Diagnostics;

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

    public static void Log(string message)
    {
        _queue.Add(message);
    }

    public static void Stop()
    {
        _queue.CompleteAdding();
        _worker.Join();
    }
}
