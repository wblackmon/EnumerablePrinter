using System.Collections;

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
        /// <typeparam name="T">Type of the elements in the collection.</typeparam>
        /// <param name="collection">The enumerable to print.</param>
        /// <param name="toString">Optional custom formatter for each element.</param>
        /// <param name="writer">Optional TextWriter (defaults to Console.Out).</param>
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
                writer.WriteLine("{ }");
                return;
            }

            toString ??= x => x?.ToString() ?? "null";

            writer.Write("{ ");
            for (int i = 0; i < items.Count; i++)
            {
                var element = items[i];
                string text;

                if (element is string s && toString == null)
                {
                    text = $"\"{s}\"";
                }
                else if (element is IEnumerable nested && element is not string && element is not byte[])
                {
                    var sw = new StringWriter();
                    PrintNested(nested, sw);
                    text = sw.ToString().Trim();
                }
                else
                {
                    text = toString((T)element!);
                }

                writer.Write(text);

                if (i < items.Count - 1)
                    writer.Write(", ");
            }
            writer.WriteLine(" }");
        }
        private static void PrintNested(IEnumerable nested, TextWriter writer)
        {
            var items = new List<object?>();
            foreach (var item in nested)
                items.Add(item);

            writer.Write("{ ");
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
            writer.Write(" }");
        }
        /// <summary>
        /// Determines whether the elements in the sequence are sorted in ascending alphabetical order.
        /// </summary>
        /// <typeparam name="T">
        /// The type of elements in the sequence. Must implement <see cref="IComparable{T}"/>.
        /// </typeparam>
        /// <param name="source">
        /// The sequence of elements to evaluate.
        /// </param>
        /// <param name="comparer">
        /// An optional comparer to define custom sorting logic. If null, the default comparer is used.
        /// </param>
        /// <returns>
        /// <c>true</c> if the sequence is empty, contains a single element, or is sorted in ascending order; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> is <c>null</c>.
        /// </exception>
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
        /// Returns a subsequence of the source collection, similar to Python's slice syntax.
        /// </summary>
        /// <typeparam name="T">
        /// The type of elements in the source sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence to slice. Must not be <c>null</c>.
        /// </param>
        /// <param name="start">
        /// The zero-based starting index of the slice. If <c>null</c>, slicing begins at the start of the sequence.
        /// Negative values are interpreted as offsets from the end.
        /// </param>
        /// <param name="end">
        /// The zero-based ending index (exclusive) of the slice. If <c>null</c>, slicing continues to the end of the sequence.
        /// Negative values are interpreted as offsets from the end.
        /// </param>
        /// <param name="step">
        /// The interval between returned elements. Must be greater than zero. Defaults to 1.
        /// </param>
        /// <returns>
        /// A new <see cref="IEnumerable{T}"/> containing the elements from <paramref name="source"/> within the specified range,
        /// taken at the specified step. If the range is empty, an empty sequence is returned.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="step"/> is less than or equal to zero.
        /// </exception>
        public static IEnumerable<T> Slice<T>(
            this IEnumerable<T> source,
            int? start = null,
            int? end = null,
            int step = 1)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (step <= 0) throw new ArgumentOutOfRangeException(nameof(step), "Step must be greater than zero.");

            var list = source as IList<T> ?? source.ToList();
            int count = list.Count;

            int from = start ?? 0;
            int to = end ?? count;

            // Handle negative indices
            if (from < 0) from = count + from;
            if (to < 0) to = count + to;

            // Clamp to valid range
            from = Math.Clamp(from, 0, count);
            to = Math.Clamp(to, 0, count);

            // If range is invalid, return empty
            if (from >= to)
                yield break;

            for (int i = from; i < to; i += step)
                yield return list[i];
        }
        /// <summary>
        /// Prints a string to the specified <see cref="TextWriter"/>, or to <see cref="Console.Out"/> if none is provided.
        /// </summary>
        /// <param name="s">The string to print. If <c>null</c>, prints <c>null</c>.</param>
        /// <param name="writer">Optional output stream. Defaults to <see cref="Console.Out"/>.</param>
        public static void Print(this string? s, TextWriter? writer = null)
        {
            writer ??= Console.Out;
            writer.WriteLine(s == null ? "null" : $"\"{s}\"");
        }

        /// <summary>
        /// Prints a summary of a byte array to the specified <see cref="TextWriter"/>, or to <see cref="Console.Out"/> if none is provided.
        /// </summary>
        /// <param name="bytes">The byte array to print. If <c>null</c>, prints <c>null</c>; otherwise prints <c>byte[n]</c> where <c>n</c> is the array length.</param>
        /// <param name="writer">Optional output stream. Defaults to <see cref="Console.Out"/>.</param>
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
        /// Prints the contents of a dictionary in a JSON‑like format.
        /// </summary>
        /// <typeparam name="TKey">The type of dictionary keys.</typeparam>
        /// <typeparam name="TValue">The type of dictionary values.</typeparam>
        /// <param name="dict">The dictionary to print. If <c>null</c>, prints <c>null</c>.</param>
        /// <param name="keyToString">Optional formatter for keys. Defaults to <c>ToString()</c>.</param>
        /// <param name="valueToString">Optional formatter for values. Defaults to <c>ToString()</c>.</param>
        /// <param name="writer">Optional output stream. Defaults to <see cref="Console.Out"/>.</param>
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
                writer.WriteLine("{ }");
                return;
            }

            keyToString ??= k => k.ToString() ?? "null";
            valueToString ??= v => v.ToString() ?? "null";

            writer.Write("{ ");
            int i = 0;
            foreach (var kvp in dictionary)
            {
                writer.Write($"\"{keyToString(kvp.Key)}\": {valueToString(kvp.Value)}");
                if (i < dictionary.Count - 1)
                {
                    writer.Write(", ");
                }
                i++;
            }
            writer.WriteLine(" }");
        }
    }
}
