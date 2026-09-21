using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using EnumerablePrinter.Abstractions;

namespace EnumerablePrinter.Formatters;

/// <summary>
/// Formats objects, collections, dictionaries, and scalar values using the default printer rules.
/// </summary>
public sealed class DefaultObjectFormatter : IObjectFormatter
{
    /// <summary>
    /// Formats a value without appending a newline.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="options">The formatting options to apply.</param>
    /// <returns>The formatted value.</returns>
    public string Format(object? value, PrintOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.MaxDepth < 0)
            throw new ArgumentOutOfRangeException(nameof(options.MaxDepth));
        if (options.MaxItems < 0)
            throw new ArgumentOutOfRangeException(nameof(options.MaxItems));
        if (options.IndentSize < 0)
            throw new ArgumentOutOfRangeException(nameof(options.IndentSize));

        var writer = new StringWriter();
        var activeReferences = new HashSet<object>(ReferenceEqualityComparer.Instance);

        WriteValue(value, writer, options, activeReferences, depth: 0);
        return writer.ToString();
    }

    private static void WriteValue(object? value, TextWriter writer, PrintOptions options, HashSet<object> activeReferences, int depth)
    {
        if (depth > options.MaxDepth)
        {
            writer.Write("<max depth>");
            return;
        }

        if (value is null)
        {
            if (options.IncludeNulls)
                writer.Write("null");
            return;
        }

        if (value is string s)
        {
            writer.Write($"\"{s}\"");
            return;
        }

        if (value is byte[] bytes)
        {
            WriteByteArray(bytes, writer);
            return;
        }

        if (value is IDictionary dictionary)
        {
            WriteDictionary(dictionary, writer, options, activeReferences, depth);
            return;
        }

        if (value is IEnumerable enumerable && value is not string)
        {
            WriteEnumerable(enumerable, writer, options, activeReferences, depth);
            return;
        }

        if (IsScalarValueType(value))
        {
            writer.Write($"\"{value}\"");
            return;
        }

        WriteObject(value, writer, options, activeReferences, depth);
    }

    private static bool IsScalarValueType(object value)
    {
        return value is ValueType &&
               value is not int &&
               value is not long &&
               value is not short &&
               value is not double &&
               value is not float &&
               value is not decimal &&
               value is not bool &&
               value is not char;
    }

    private static void WriteEnumerable(IEnumerable seq, TextWriter writer, PrintOptions options, HashSet<object> activeReferences, int depth)
    {
        if (!TryEnter(seq, activeReferences, writer))
            return;

        var enumerator = seq.GetEnumerator();
        try
        {
            if (!enumerator.MoveNext())
            {
                writer.Write("[ ]");
                return;
            }

            writer.Write("[");

            bool first = true;
            int count = 0;

            do
            {
                var item = enumerator.Current;
                if (item is not null || options.IncludeNulls)
                {
                    if (count++ >= options.MaxItems)
                    {
                        writer.Write(" <max items> ");
                        break;
                    }

                    if (!first)
                        writer.Write(", ");
                    first = false;

                    if (options.Pretty)
                    {
                        writer.WriteLine();
                        writer.Write(new string(' ', (depth + 1) * options.IndentSize));
                    }

                    WriteValue(item, writer, options, activeReferences, depth + 1);
                }
            }
            while (enumerator.MoveNext());

            if (options.Pretty && !first)
            {
                writer.WriteLine();
                writer.Write(new string(' ', depth * options.IndentSize));
            }

            writer.Write("]");
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
            activeReferences.Remove(seq);
        }
    }

    private static void WriteDictionary(IDictionary dict, TextWriter writer, PrintOptions options, HashSet<object> activeReferences, int depth)
    {
        if (!TryEnter(dict, activeReferences, writer))
            return;

        if (dict.Count == 0)
        {
            writer.Write("{ }");
            activeReferences.Remove(dict);
            return;
        }

        writer.Write("{");

        bool first = true;
        int count = 0;

        foreach (DictionaryEntry entry in dict)
        {
            if (entry.Value is null && !options.IncludeNulls)
                continue;

            if (count++ >= options.MaxItems)
            {
                writer.Write(" <max items> ");
                break;
            }

            if (!first)
                writer.Write(", ");
            first = false;

            if (options.Pretty)
            {
                writer.WriteLine();
                writer.Write(new string(' ', (depth + 1) * options.IndentSize));
            }

            WriteValue(entry.Key, writer, options, activeReferences, depth + 1);
            writer.Write(": ");
            WriteValue(entry.Value, writer, options, activeReferences, depth + 1);
        }

        if (options.Pretty && !first)
        {
            writer.WriteLine();
            writer.Write(new string(' ', depth * options.IndentSize));
        }

        writer.Write("}");
        activeReferences.Remove(dict);
    }

    private static void WriteByteArray(byte[] bytes, TextWriter writer)
    {
        if (bytes.Length == 0)
        {
            writer.Write("[ ]");
            return;
        }

        writer.Write("[");
        for (var index = 0; index < bytes.Length; index++)
        {
            writer.Write(bytes[index]);
            if (index < bytes.Length - 1)
                writer.Write(", ");
        }
        writer.Write("]");
    }

    private static void WriteObject(object obj, TextWriter writer, PrintOptions options, HashSet<object> activeReferences, int depth)
    {
        if (!TryEnter(obj, activeReferences, writer))
            return;

        var type = obj.GetType();

        var props = type.GetProperties(
            BindingFlags.Public | BindingFlags.Instance |
            (options.IncludePrivateProperties ? BindingFlags.NonPublic : 0))
            .Where(property => property.GetIndexParameters().Length == 0)
            .ToArray();

        var fields = type.GetFields(
            BindingFlags.Public | BindingFlags.Instance |
            (options.IncludePrivateFields ? BindingFlags.NonPublic : 0))
            .Where(field => !field.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false))
            .ToArray();

        if (props.Length == 0 && fields.Length == 0)
        {
            writer.Write(obj.ToString());
            activeReferences.Remove(obj);
            return;
        }

        writer.Write("{");

        bool first = true;

        foreach (var p in props)
        {
            var val = SafeGet(() => p.GetValue(obj));
            if (val is null && !options.IncludeNulls)
                continue;

            if (!first)
                writer.Write(", ");
            first = false;

            if (options.Pretty)
            {
                writer.WriteLine();
                writer.Write(new string(' ', (depth + 1) * options.IndentSize));
            }

            writer.Write(p.Name);
            writer.Write(": ");

            WriteValue(val, writer, options, activeReferences, depth + 1);
        }

        foreach (var f in fields)
        {
            var val = SafeGet(() => f.GetValue(obj));
            if (val is null && !options.IncludeNulls)
                continue;

            if (!first)
                writer.Write(", ");
            first = false;

            if (options.Pretty)
            {
                writer.WriteLine();
                writer.Write(new string(' ', (depth + 1) * options.IndentSize));
            }

            writer.Write(f.Name);
            writer.Write(": ");

            WriteValue(val, writer, options, activeReferences, depth + 1);
        }

        if (options.Pretty && !first)
        {
            writer.WriteLine();
            writer.Write(new string(' ', depth * options.IndentSize));
        }

        writer.Write("}");
        activeReferences.Remove(obj);
    }

    private static bool TryEnter(object obj, HashSet<object> activeReferences, TextWriter writer)
    {
        if (!activeReferences.Add(obj))
        {
            writer.Write("<Circular Reference>");
            return false;
        }

        return true;
    }

    private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceEqualityComparer Instance = new();

        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);

        public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
    }

    private static object? SafeGet(Func<object?> getter)
    {
        try { return getter(); }
        catch { return "<error>"; }
    }
}
