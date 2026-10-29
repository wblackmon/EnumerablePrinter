using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using EnumerablePrinter.Abstractions;

namespace EnumerablePrinter.Formatters;

/// <summary>
/// Formats objects, collections, dictionaries, and scalar values
/// using the default printer rules.
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
        // ArgumentNullException.ThrowIfNull is not available in netstandard2.0.
        if (options is null)
            throw new ArgumentNullException(nameof(options));

        if (options.MaxDepth < 0)
            throw new ArgumentOutOfRangeException(nameof(options.MaxDepth));

        if (options.MaxItems < 0)
            throw new ArgumentOutOfRangeException(nameof(options.MaxItems));

        if (options.IndentSize < 0)
            throw new ArgumentOutOfRangeException(nameof(options.IndentSize));

        using var writer = new StringWriter();

        var activeReferences =
            new HashSet<object>(ReferenceEqualityComparer.Instance);

        WriteValue(
            value,
            writer,
            options,
            activeReferences,
            depth: 0);

        return writer.ToString();
    }

    private static void WriteValue(
        object? value,
        TextWriter writer,
        PrintOptions options,
        HashSet<object> activeReferences,
        int depth)
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

        if (TryWriteScalar(value, writer))
            return;

        if (value is byte[] bytes)
        {
            WriteByteArray(bytes, writer);
            return;
        }

        if (value is IDictionary dictionary)
        {
            WriteDictionary(
                dictionary,
                writer,
                options,
                activeReferences,
                depth);

            return;
        }

        if (value is IEnumerable enumerable)
        {
            WriteEnumerable(
                enumerable,
                writer,
                options,
                activeReferences,
                depth);

            return;
        }

        WriteObject(
            value,
            writer,
            options,
            activeReferences,
            depth);
    }

    /// <summary>
    /// Writes built-in scalar values using C#-style display rules.
    /// C# literal suffixes such as m, f, d, and L are not included.
    /// Returns false for complex objects and user-defined structures.
    /// </summary>
    private static bool TryWriteScalar(
        object value,
        TextWriter writer)
    {
        switch (value)
        {
            // Text types
            case string text:
                WriteQuotedString(text, writer);
                return true;

            case char character:
                WriteQuotedChar(character, writer);
                return true;

            // Boolean
            case bool boolean:
                writer.Write(boolean ? "true" : "false");
                return true;

            // Signed integral types
            case sbyte number:
                writer.Write(number);
                return true;

            case short number:
                writer.Write(number);
                return true;

            case int number:
                writer.Write(number);
                return true;

            case long number:
                writer.Write(number);
                return true;

            // Unsigned integral types
            case byte number:
                writer.Write(number);
                return true;

            case ushort number:
                writer.Write(number);
                return true;

            case uint number:
                writer.Write(number);
                return true;

            case ulong number:
                writer.Write(number);
                return true;

            // Native-sized integral types
            case IntPtr number:
                writer.Write(number);
                return true;

            case UIntPtr number:
                writer.Write(number);
                return true;

            // Floating-point and decimal types
            case float number:
                writer.Write(number);
                return true;

            case double number:
                writer.Write(number);
                return true;

            case decimal number:
                writer.Write(number);
                return true;

            // Date and time types available in netstandard2.0
            case DateTime dateTime:
                writer.Write(dateTime);
                return true;

            case DateTimeOffset dateTimeOffset:
                writer.Write(dateTimeOffset);
                return true;

            case TimeSpan duration:
                writer.Write(duration);
                return true;

            // Other common scalar types
            case Guid guid:
                writer.Write(guid);
                return true;

            // All enum types
            case Enum enumValue:
                writer.Write(enumValue);
                return true;
        }

        /*
         * System.Half, System.DateOnly, and System.TimeOnly are not
         * available in the netstandard2.0 reference assemblies.
         *
         * Checking their fully qualified runtime names allows the
         * netstandard2.0 build to recognize them when it is consumed
         * by an application running on a newer .NET runtime.
         */
        switch (value.GetType().FullName)
        {
            case "System.Half":
            case "System.DateOnly":
            case "System.TimeOnly":
                writer.Write(value);
                return true;

            default:
                return false;
        }
    }

    private static void WriteEnumerable(
        IEnumerable sequence,
        TextWriter writer,
        PrintOptions options,
        HashSet<object> activeReferences,
        int depth)
    {
        if (!TryEnter(sequence, activeReferences, writer))
            return;

        var enumerator = sequence.GetEnumerator();

        try
        {
            if (!enumerator.MoveNext())
            {
                writer.Write("[ ]");
                return;
            }

            writer.Write("[");

            var first = true;
            var count = 0;

            do
            {
                var item = enumerator.Current;

                if (item is null && !options.IncludeNulls)
                    continue;

                if (count >= options.MaxItems)
                {
                    // Preserve the package's existing max-items format.
                    writer.Write(" <max items> ");
                    break;
                }

                count++;

                if (!first)
                    writer.Write(", ");

                first = false;

                if (options.Pretty)
                {
                    writer.WriteLine();
                    WriteIndent(writer, depth + 1, options.IndentSize);
                }

                WriteValue(
                    item,
                    writer,
                    options,
                    activeReferences,
                    depth + 1);
            }
            while (enumerator.MoveNext());

            if (options.Pretty && !first)
            {
                writer.WriteLine();
                WriteIndent(writer, depth, options.IndentSize);
            }

            writer.Write("]");
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
            activeReferences.Remove(sequence);
        }
    }

    private static void WriteDictionary(
        IDictionary dictionary,
        TextWriter writer,
        PrintOptions options,
        HashSet<object> activeReferences,
        int depth)
    {
        if (!TryEnter(dictionary, activeReferences, writer))
            return;

        try
        {
            if (dictionary.Count == 0)
            {
                writer.Write("{ }");
                return;
            }

            writer.Write("{");

            var first = true;
            var count = 0;

            foreach (DictionaryEntry entry in dictionary)
            {
                if (entry.Value is null && !options.IncludeNulls)
                    continue;

                if (count >= options.MaxItems)
                {
                    // Preserve the package's existing max-items format.
                    writer.Write(" <max items> ");
                    break;
                }

                count++;

                if (!first)
                    writer.Write(", ");

                first = false;

                if (options.Pretty)
                {
                    writer.WriteLine();
                    WriteIndent(writer, depth + 1, options.IndentSize);
                }

                WriteValue(
                    entry.Key,
                    writer,
                    options,
                    activeReferences,
                    depth + 1);

                writer.Write(": ");

                WriteValue(
                    entry.Value,
                    writer,
                    options,
                    activeReferences,
                    depth + 1);
            }

            if (options.Pretty && !first)
            {
                writer.WriteLine();
                WriteIndent(writer, depth, options.IndentSize);
            }

            writer.Write("}");
        }
        finally
        {
            activeReferences.Remove(dictionary);
        }
    }

    private static void WriteByteArray(
        byte[] bytes,
        TextWriter writer)
    {
        if (bytes.Length == 0)
        {
            writer.Write("[ ]");
            return;
        }

        writer.Write("[");

        for (var index = 0; index < bytes.Length; index++)
        {
            if (index > 0)
                writer.Write(", ");

            writer.Write(bytes[index]);
        }

        writer.Write("]");
    }

    private static void WriteObject(
        object value,
        TextWriter writer,
        PrintOptions options,
        HashSet<object> activeReferences,
        int depth)
    {
        if (!TryEnter(value, activeReferences, writer))
            return;

        try
        {
            var type = value.GetType();

            var properties = type
                .GetProperties(
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    (options.IncludePrivateProperties
                        ? BindingFlags.NonPublic
                        : 0))
                .Where(property =>
                    property.GetIndexParameters().Length == 0)
                .ToArray();

            var fields = type
                .GetFields(
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    (options.IncludePrivateFields
                        ? BindingFlags.NonPublic
                        : 0))
                .Where(field =>
                    !field.IsDefined(
                        typeof(CompilerGeneratedAttribute),
                        inherit: false))
                .ToArray();

            if (properties.Length == 0 && fields.Length == 0)
            {
                writer.Write(value);
                return;
            }

            writer.Write("{");

            var first = true;

            foreach (var property in properties)
            {
                var propertyValue = SafeGet(
                    () => property.GetValue(value));

                if (propertyValue is null && !options.IncludeNulls)
                    continue;

                WriteMemberPrefix(
                    property.Name,
                    writer,
                    options,
                    depth,
                    ref first);

                WriteValue(
                    propertyValue,
                    writer,
                    options,
                    activeReferences,
                    depth + 1);
            }

            foreach (var field in fields)
            {
                var fieldValue = SafeGet(
                    () => field.GetValue(value));

                if (fieldValue is null && !options.IncludeNulls)
                    continue;

                WriteMemberPrefix(
                    field.Name,
                    writer,
                    options,
                    depth,
                    ref first);

                WriteValue(
                    fieldValue,
                    writer,
                    options,
                    activeReferences,
                    depth + 1);
            }

            if (options.Pretty && !first)
            {
                writer.WriteLine();
                WriteIndent(writer, depth, options.IndentSize);
            }

            writer.Write("}");
        }
        finally
        {
            activeReferences.Remove(value);
        }
    }

    private static void WriteMemberPrefix(
        string memberName,
        TextWriter writer,
        PrintOptions options,
        int depth,
        ref bool first)
    {
        if (!first)
            writer.Write(", ");

        first = false;

        if (options.Pretty)
        {
            writer.WriteLine();
            WriteIndent(writer, depth + 1, options.IndentSize);
        }

        writer.Write(memberName);
        writer.Write(": ");
    }

    private static void WriteQuotedString(
        string value,
        TextWriter writer)
    {
        writer.Write('"');

        foreach (var character in value)
        {
            WriteEscapedCharacter(
                character,
                writer,
                quoteCharacter: '"');
        }

        writer.Write('"');
    }

    private static void WriteQuotedChar(
        char value,
        TextWriter writer)
    {
        writer.Write('\'');

        WriteEscapedCharacter(
            value,
            writer,
            quoteCharacter: '\'');

        writer.Write('\'');
    }

    private static void WriteEscapedCharacter(
        char value,
        TextWriter writer,
        char quoteCharacter)
    {
        switch (value)
        {
            case '\\':
                writer.Write(@"\\");
                break;

            case '"' when quoteCharacter == '"':
                writer.Write("\\\"");
                break;

            case '\'' when quoteCharacter == '\'':
                writer.Write("\\'");
                break;

            case '\0':
                writer.Write(@"\0");
                break;

            case '\a':
                writer.Write(@"\a");
                break;

            case '\b':
                writer.Write(@"\b");
                break;

            case '\f':
                writer.Write(@"\f");
                break;

            case '\n':
                writer.Write(@"\n");
                break;

            case '\r':
                writer.Write(@"\r");
                break;

            case '\t':
                writer.Write(@"\t");
                break;

            case '\v':
                writer.Write(@"\v");
                break;

            default:
                writer.Write(value);
                break;
        }
    }

    private static void WriteIndent(
        TextWriter writer,
        int depth,
        int indentSize)
    {
        writer.Write(new string(' ', depth * indentSize));
    }

    private static bool TryEnter(
        object value,
        HashSet<object> activeReferences,
        TextWriter writer)
    {
        if (activeReferences.Add(value))
            return true;

        writer.Write("<Circular Reference>");
        return false;
    }

    private static object? SafeGet(
        Func<object?> getter)
    {
        try
        {
            return getter();
        }
        catch
        {
            return "<error>";
        }
    }

    private sealed class ReferenceEqualityComparer
        : IEqualityComparer<object>
    {
        public static readonly ReferenceEqualityComparer Instance = new();

        private ReferenceEqualityComparer()
        {
        }

        public new bool Equals(
            object? x,
            object? y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(object value)
        {
            return RuntimeHelpers.GetHashCode(value);
        }
    }
}