using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

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
            this IEnumerable<T>? collection,
            Func<T, string>? toString = null,
            TextWriter? writer = null)
        {
            writer ??= Console.Out;
            toString ??= item => item?.ToString() ?? "null";

            if (collection == null || !collection.Any())
            {
                writer.WriteLine("{ }");
                return;
            }

            var list = collection.ToList();

            writer.Write("{ ");
            for (int i = 0; i < list.Count; i++)
            {
                writer.Write(toString(list[i]));
                if (i < list.Count - 1)
                    writer.Write(", ");
            }
            writer.WriteLine(" }");
        }
        public static bool IsAlphabetical(string input)
        {
            for (int i = 1; i < input.Length; i++)
            {
                if (input[i] < input[i - 1])
                    return false;
            }
            return true;
        }
    }
}
