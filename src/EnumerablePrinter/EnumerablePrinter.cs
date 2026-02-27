using System.Collections;
using System.Reflection;

namespace EnumerablePrinter
{
    /// <summary>
    /// Provides extension methods for printing sequences, dictionaries, objects,
    /// and nested structures in a readable, Python-inspired format.
    /// </summary>
    public static class EnumerableExtensions
    {
        // =====================================================================
        //  IEnumerable<T> Printer
        // =====================================================================

        /// <summary>
        /// Prints the contents of an <see cref="IEnumerable{T}"/> using optional
        /// formatting and an optional output writer. Supports nested collections,
        /// strings, primitives, and complex objects.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="source">The sequence to print.</param>
        /// <param name="toString">Optional formatter for each element.</param>
        /// <param name="writer">Optional output writer (defaults to Console.Out).</param>
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

            // Special case: treat IEnumerable<char> as a string literal
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
                // Complex objects → reflect properties
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
                    text = toString((T)element!);
                }

                writer.Write(text);

                if (i < items.Count - 1)
                    writer.Write(", ");
            }
            writer.WriteLine(" ]");
        }

        /// <summary>
        /// Prints a nested <see cref="IEnumerable"/> recursively.
        /// </summary>
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

        // =====================================================================
        //  Utility Extensions
        // =====================================================================

        /// <summary>
        /// Determines whether the sequence is sorted alphabetically using a selector.
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
        /// Performs Python-style slicing on a sequence.
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

        /// <summary>
        /// Prints a string as a quoted literal.
        /// </summary>
        public static void Print(this string? s, TextWriter? writer = null)
        {
            writer ??= Console.Out;
            writer.WriteLine(s == null ? "null" : $"\"{s}\"");
        }

        /// <summary>
        /// Prints a byte array as a length descriptor.
        /// </summary>
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
        /// Prints a dictionary using square-bracket formatting.
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
        /// Splits a sequence into fixed-size chunks.
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

        // =====================================================================
        //  Dynamic Printer (Universal Entry Point)
        // =====================================================================

        /// <summary>
        /// Prints any object by dynamically selecting the appropriate printer:
        /// string, byte array, dictionary, enumerable, or reflection-based object printer.
        /// </summary>
        public static void Print(this object? obj, TextWriter? writer = null)
        {
            PrintDynamic(obj, writer ?? Console.Out);
        }

        /// <summary>
        /// Routes an object to the correct printer based on implemented interfaces.
        /// </summary>
        private static void PrintDynamic(object? obj, TextWriter writer)
        {
            switch (obj)
            {
                case null:
                    writer.WriteLine("null");
                    return;

                case string s:
                    s.Print(writer);
                    return;

                case byte[] bytes:
                    bytes.Print(writer);
                    return;

                case IDictionary dict:
                    PrintDictionaryDynamic(dict, writer);
                    return;

                case IEnumerable enumerable when obj is not string:
                    PrintEnumerableDynamic(enumerable, writer);
                    return;

                default:
                    PrintObjectDynamic(obj, writer);
                    return;
            }
        }

        /// <summary>
        /// Prints any <see cref="IDictionary"/> dynamically, supporting nested dictionaries
        /// and nested enumerables.
        /// </summary>
        private static void PrintDictionaryDynamic(IDictionary dict, TextWriter writer)
        {
            if (dict.Count == 0)
            {
                writer.WriteLine("[ ]");
                return;
            }

            writer.Write("[ ");

            int i = 0;
            foreach (DictionaryEntry entry in dict)
            {
                writer.Write($"\"{entry.Key}\": ");

                if (entry.Value is IDictionary nestedDict)
                {
                    PrintDictionaryDynamic(nestedDict, writer);
                }
                else if (entry.Value is IEnumerable nested && entry.Value is not string)
                {
                    PrintEnumerableDynamic(nested, writer);
                }
                else
                {
                    writer.Write(entry.Value?.ToString() ?? "null");
                }

                if (i < dict.Count - 1)
                    writer.Write(", ");

                i++;
            }

            writer.WriteLine(" ]");
        }

        /// <summary>
        /// Prints any <see cref="IEnumerable"/> dynamically, supporting nested collections,
        /// dictionaries, and primitive values.
        /// </summary>
        private static void PrintEnumerableDynamic(IEnumerable source, TextWriter writer)
        {
            var items = source.Cast<object?>().ToList();

            if (items.Count == 0)
            {
                writer.WriteLine("[ ]");
                return;
            }

            writer.Write("[ ");

            for (int i = 0; i < items.Count; i++)
            {
                var element = items[i];

                switch (element)
                {
                    case null:
                        writer.Write("null");
                        break;

                    case string s:
                        writer.Write($"\"{s}\"");
                        break;

                    case IDictionary dict:
                        PrintDictionaryDynamic(dict, writer);
                        break;

                    case IEnumerable nested when element is not string:
                        PrintEnumerableDynamic(nested, writer);
                        break;

                    default:
                        writer.Write(element.ToString());
                        break;
                }

                if (i < items.Count - 1)
                    writer.Write(", ");
            }

            writer.Write(" ]");
        }

        /// <summary>
        /// Prints a complex object by reflecting its public instance properties.
        /// Supports nested dictionaries and nested enumerables.
        /// </summary>
        private static void PrintObjectDynamic(object obj, TextWriter writer)
        {
            var type = obj.GetType();
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            if (props.Length == 0)
            {
                writer.WriteLine(obj.ToString());
                return;
            }

            writer.Write("{ ");

            for (int i = 0; i < props.Length; i++)
            {
                var p = props[i];
                var value = p.GetValue(obj);

                writer.Write($"{p.Name}: ");

                switch (value)
                {
                    case null:
                        writer.Write("null");
                        break;

                    case IDictionary dict:
                        PrintDictionaryDynamic(dict, writer);
                        break;

                    case IEnumerable enumerable when value is not string:
                        PrintEnumerableDynamic(enumerable, writer);
                        break;

                    default:
                        writer.Write(value.ToString());
                        break;
                }

                if (i < props.Length - 1)
                    writer.Write(", ");
            }

            writer.WriteLine(" }");
        }
    }
}
