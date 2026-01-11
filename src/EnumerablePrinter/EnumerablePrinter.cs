using System.Collections;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics.Metrics;
using System.Runtime.InteropServices;
using System.Reflection;

namespace EnumerablePrinter
{
    /// <summary>
    /// Extension methods for printing IEnumerable collections, inspired by Python's print().
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Prints the contents of an IEnumerable using optional formatting and output stream.
        /// </summary>
        public static void Print<T>(
            this IEnumerable<T>? source,
            Func<T, string>? toString = null,
            TextWriter? writer = null)
        {
            writer ??= Console.Out;

            if (source == null)
            {
                writer.WriteLine("null");
                return;
            }

            // Special case: char sequences → print as a string literal
            if (source is IEnumerable<char> chars)
            {
                writer.WriteLine($"\"{new string(chars.ToArray())}\"");
                return;
            }

            var items = source.ToList();
            if (items.Count == 0)
            {
                writer.WriteLine("[ ]");
                return;
            }

            toString ??= x => x?.ToString() ?? "null";

            writer.Write("[ ");
            for (int i = 0; i < items.Count; i++)
            {
                var element = items[i];
                string text;

                // Strings
                if (element is string s && toString == null)
                {
                    text = $"\"{s}\"";
                }
                // Nested collections
                else if (element is IEnumerable nested && element is not string && element is not byte[])
                {
                    var sw = new StringWriter();
                    PrintNested(nested, sw);
                    text = sw.ToString().Trim();
                }
                // Complex objects → print properties
                else if (element is not null &&
                         !element.GetType().IsPrimitive &&
                         element is not string &&
                         element is not byte[] &&
                         element is not IEnumerable)
                {
                    var type = element.GetType();
                    var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    if (props.Length > 0)
                    {
                        var parts = props.Select(p =>
                        {
                            var value = p.GetValue(element);
                            return $"{p.Name}: {value}";
                        });

                        text = "[ " + string.Join(", ", parts) + " ]";
                    }
                    else
                    {
                        text = element.ToString() ?? "null";
                    }
                }
                else
                {
                    text = (toString ?? (x => x?.ToString() ?? "null"))((T)element!);
                }

                writer.Write(text);

                if (i < items.Count - 1)
                    writer.Write(", ");
            }
            writer.WriteLine(" ]");
        }

        private static void PrintNested(IEnumerable nested, TextWriter writer)
        {
            var items = new List<object?>();
            foreach (var item in nested)
                items.Add(item);

            writer.Write("[ ");
            for (int i = 0; i < items.Count; i++)
            {
                var element = items[i];
                string text;

                if (element is string s)
                {
                    text = $"\"{s}\"";
                }
                else if (element is IEnumerable inner && element is not string && element is not byte[])
                {
                    var sw = new StringWriter();
                    PrintNested(inner, sw);
                    text = sw.ToString().Trim();
                }
                else
                {
                    text = element?.ToString() ?? "null";
                }

                writer.Write(text);

                if (i < items.Count - 1)
                    writer.Write(", ");
            }
            writer.Write(" ]");
        }

        /// <summary>
        /// Determines whether the elements in the sequence are sorted alphabetically.
        /// </summary>
        public static bool IsAlphabetical<T>(this IEnumerable<T> source, Func<T, string> selector, IComparer<string>? comparer = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            comparer ??= StringComparer.Ordinal;

            using var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext()) return true;

            string previous = selector(enumerator.Current);
            while (enumerator.MoveNext())
            {
                string current = selector(enumerator.Current);
                if (comparer.Compare(previous, current) > 0) return false;
                previous = current;
            }

            return true;
        }

        /// <summary>
        /// Python-style slice.
        /// </summary>
        public static IEnumerable<T> Slice<T>(
            this IEnumerable<T> source,
            int? start = null,
            int? end = null,
            int step = 1)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (step <= 0) throw new ArgumentOutOfRangeException(nameof(step));

            var list = source as IList<T> ?? source.ToList();
            int count = list.Count;

            int from = start ?? 0;
            int to = end ?? count;

            if (from < 0) from = count + from;
            if (to < 0) to = count + to;

            from = Math.Clamp(from, 0, count);
            to = Math.Clamp(to, 0, count);

            if (from >= to)
                yield break;

            for (int i = from; i < to; i += step)
                yield return list[i];
        }

        public static void Print(this string? s, TextWriter? writer = null)
        {
            writer ??= Console.Out;
            writer.WriteLine(s == null ? "null" : $"\"{s}\"");
        }

        public static void Print(this byte[]? bytes, TextWriter? writer = null)
        {
            writer ??= Console.Out;
            if (bytes == null)
            {
                writer.WriteLine("null");
                return;
            }
            writer.WriteLine($"byte[{bytes.Length}]");
        }

        /// <summary>
        /// Dictionary printing (square brackets).
        /// </summary>
        public static void Print<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            Func<TKey, string>? keyToString = null,
            Func<TValue, string>? valueToString = null,
            TextWriter? writer = null)
        {
            writer ??= Console.Out;

            if (dictionary == null)
            {
                writer.WriteLine("null");
                return;
            }

            if (dictionary.Count == 0)
            {
                writer.WriteLine("[ ]");
                return;
            }

            keyToString ??= k => k?.ToString() ?? "null";
            valueToString ??= v => v?.ToString() ?? "null";

            writer.Write("[ ");
            int i = 0;
            foreach (var kvp in dictionary)
            {
                writer.Write($"\"{keyToString(kvp.Key)}\": {valueToString(kvp.Value)}");
                if (i < dictionary.Count - 1)
                    writer.Write(", ");
                i++;
            }
            writer.WriteLine(" ]");
        }

        /// <summary>
        /// Chunk implementation.
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T>? source, int size)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            using var iter = source.GetEnumerator();

            while (true)
            {
                var chunk = new List<T>(size);

                for (int i = 0; i < size && iter.MoveNext(); i++)
                    chunk.Add(iter.Current);

                if (chunk.Count == 0)
                    yield break;

                yield return chunk;
            }
        }
    }
}
