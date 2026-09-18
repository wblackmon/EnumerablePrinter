using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using EnumerablePrinter.Abstractions;

namespace EnumerablePrinter.Formatters;

public sealed class DefaultObjectFormatter : IObjectFormatter
{
    private const int MaxDepth = 32;

    public string Format(object? value)
    {
        var writer = new StringWriter();
        var active = new HashSet<object>(ReferenceEqualityComparer.Instance);

        WriteValue(value, writer, active, 0);
        return writer.ToString();
    }

    private void WriteValue(object? value, TextWriter writer, HashSet<object> activeReferences, int depth)
    {
        if (depth > MaxDepth)
        {
            writer.Write("<Depth Limit>");
            return;
        }

        if (value is null)
        {
            writer.Write("null");
            return;
        }

        if (value is string s)
        {
            WriteQuotedString(s, writer);
            return;
        }

        if (value is byte[] bytes)
        {
            WriteByteArray(bytes, writer);
            return;
        }

        if (value is IDictionary dictionary)
        {
            WriteDictionary(dictionary, writer, activeReferences, depth);
            return;
        }

        if (value is IEnumerable enumerable && value is not string)
        {
            WriteEnumerable(enumerable, writer, activeReferences, depth);
            return;
        }

        if (IsScalarValueType(value))
        {
            writer.Write($"\"{value}\"");
            return;
        }

        WriteObject(value, writer, activeReferences, depth);
    }

    private static void WriteQuotedString(string value, TextWriter writer)
    {
        writer.Write($"\"{value}\"");
    }

    private static bool IsScalarValueType(object value)
    {
        return value is ValueType &&
               value is not byte[] &&
               value is not int &&
               value is not long &&
               value is not short &&
               value is not double &&
               value is not float &&
               value is not decimal &&
               value is not bool &&
               value is not char;
    }

    private void WriteEnumerable(IEnumerable source, TextWriter writer, HashSet<object> activeReferences, int depth)
    {
        if (!TryEnter(source, activeReferences, writer))
            return;

        var items = source.Cast<object?>().ToList();
        if (items.Count == 0)
        {
            writer.Write("[ ]");
            activeReferences.Remove(source);
            return;
        }

        writer.Write("[");
        for (var i = 0; i < items.Count; i++)
        {
            WriteValue(items[i], writer, activeReferences, depth + 1);
            if (i < items.Count - 1)
                writer.Write(", ");
        }

        writer.Write("]");
        activeReferences.Remove(source);
    }

    private void WriteDictionary(IDictionary dict, TextWriter writer, HashSet<object> activeReferences, int depth)
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
        var index = 0;

        foreach (DictionaryEntry entry in dict)
        {
            writer.Write($"\"{entry.Key}\": ");
            WriteValue(entry.Value, writer, activeReferences, depth + 1);

            if (index < dict.Count - 1)
                writer.Write(", ");

            index++;
        }

        writer.Write("}");
        activeReferences.Remove(dict);
    }

    private void WriteByteArray(byte[] bytes, TextWriter writer)
    {
        if (bytes.Length == 0)
        {
            writer.Write("[ ]");
            return;
        }

        writer.Write("[");
        for (var i = 0; i < bytes.Length; i++)
        {
            writer.Write(bytes[i]);
            if (i < bytes.Length - 1)
                writer.Write(", ");
        }
        writer.Write("]");
    }

    private void WriteObject(object obj, TextWriter writer, HashSet<object> activeReferences, int depth)
    {
        if (!TryEnter(obj, activeReferences, writer))
            return;

        var type = obj.GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        if (properties.Length == 0)
        {
            writer.Write(obj.ToString());
            activeReferences.Remove(obj);
            return;
        }

        writer.Write("{");
        for (var i = 0; i < properties.Length; i++)
        {
            var property = properties[i];
            var value = property.GetValue(obj);

            writer.Write($"{property.Name}: ");
            WriteValue(value, writer, activeReferences, depth + 1);

            if (i < properties.Length - 1)
                writer.Write(", ");
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
}
