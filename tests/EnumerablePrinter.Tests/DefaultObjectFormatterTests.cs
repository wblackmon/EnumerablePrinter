namespace EnumerablePrinter.Tests;

using EnumerablePrinter;
using EnumerablePrinter.Abstractions;
using EnumerablePrinter.Extensions;

[TestClass]
public class DefaultObjectFormatterTests
{
    private StringWriter _writer = null!;

    [TestInitialize]
    public void TestSetup()
    {
        _writer = new StringWriter();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _writer.Dispose();
    }

    [TestMethod]
    public void PrintToConsole_WritesToConsoleOutput()
    {
        var original = Console.Out;

        try
        {
            Console.SetOut(_writer);

            var value = new[] { 1, 2, 3 };
            value.PrintToConsole();

            Assert.AreEqual(
                "[1, 2, 3]\n",
                NormalizeNewlines(_writer.ToString()));
        }
        finally
        {
            Console.SetOut(original);
        }
    }

    [TestMethod]
    public void Print_WithCircularReference_PrintsCircularReferenceMarker()
    {
        var list = new List<object>();
        list.Add(list);

        using var writer = new StringWriter();

        list.Print(writer);

        Assert.AreEqual(
            "[<Circular Reference>]\n",
            NormalizeNewlines(writer.ToString()));
    }

    [TestMethod]
    public void ObjectFormatter_Format_DoesNotAppendNewline()
    {
        var result = ObjectFormatter.Format(new[] { 1, 2, 3 });

        Assert.AreEqual("[1, 2, 3]", result);
    }

    [TestMethod]
    public void ObjectFormatter_Write_WritesToProvidedWriter()
    {
        using var writer = new StringWriter();

        ObjectFormatter.Write(new[] { 1, 2, 3 }, writer);

        Assert.AreEqual("[1, 2, 3]", writer.ToString());
    }

    [TestMethod]
    public void ObjectFormatter_Format_SByte_FormatsAsScalar()
    {
        var result = ObjectFormatter.Format((sbyte)-42);

        Assert.AreEqual("-42", result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_Byte_FormatsAsScalar()
    {
        var result = ObjectFormatter.Format((byte)42);

        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_Short_FormatsAsScalar()
    {
        var result = ObjectFormatter.Format((short)-42);

        Assert.AreEqual("-42", result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_UShort_FormatsAsScalar()
    {
        var result = ObjectFormatter.Format((ushort)42);

        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_Int_FormatsAsScalar()
    {
        var result = ObjectFormatter.Format(42);

        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_UInt_FormatsAsScalar()
    {
        var result = ObjectFormatter.Format(42U);

        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_Long_FormatsAsScalar()
    {
        var result = ObjectFormatter.Format(42L);

        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_ULong_FormatsAsScalar()
    {
        var result = ObjectFormatter.Format(42UL);

        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_IntPtr_FormatsAsScalar()
    {
        var result = ObjectFormatter.Format(new IntPtr(42));

        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_UIntPtr_FormatsAsScalar()
    {
        var result = ObjectFormatter.Format(new UIntPtr(42));

        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_Half_FormatsAsScalar()
    {
        Half value = (Half)1.5f;

        var result = ObjectFormatter.Format(value);

        Assert.AreEqual(value.ToString(), result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_Float_FormatsAsScalar()
    {
        const float value = 3.14f;

        var result = ObjectFormatter.Format(value);

        Assert.AreEqual(value.ToString(), result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_Double_FormatsAsScalar()
    {
        const double value = 3.14d;

        var result = ObjectFormatter.Format(value);

        Assert.AreEqual(value.ToString(), result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_Decimal_FormatsAsScalar()
    {
        const decimal value = 5.0m;

        var result = ObjectFormatter.Format(value);

        Assert.AreEqual(value.ToString(), result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_Boolean_UsesCSharpFormatting()
    {
        Assert.AreEqual(
            "true",
            ObjectFormatter.Format(true));

        Assert.AreEqual(
            "false",
            ObjectFormatter.Format(false));
    }

    [TestMethod]
    public void ObjectFormatter_Format_String_IsQuoted()
    {
        var result = ObjectFormatter.Format("hello");

        Assert.AreEqual("\"hello\"", result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_Char_IsQuoted()
    {
        var result = ObjectFormatter.Format('A');

        Assert.AreEqual("'A'", result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_String_EscapesSpecialCharacters()
    {
        var result = ObjectFormatter.Format(
            "hello\n\"world\"\\test");

        Assert.AreEqual(
            "\"hello\\n\\\"world\\\"\\\\test\"",
            result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_Char_EscapesSingleQuote()
    {
        var result = ObjectFormatter.Format('\'');

        Assert.AreEqual("'\\''", result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_Char_EscapesNewline()
    {
        var result = ObjectFormatter.Format('\n');

        Assert.AreEqual("'\\n'", result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_Enum_FormatsAsScalar()
    {
        var result = ObjectFormatter.Format(DayOfWeek.Monday);

        Assert.AreEqual("Monday", result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_DateTime_FormatsAsScalar()
    {
        var value = new DateTime(
            2024,
            1,
            1,
            12,
            30,
            0,
            DateTimeKind.Utc);

        var result = ObjectFormatter.Format(value);

        Assert.AreEqual(value.ToString(), result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_DateTimeOffset_FormatsAsScalar()
    {
        var value = new DateTimeOffset(
            2024,
            1,
            1,
            12,
            30,
            0,
            TimeSpan.Zero);

        var result = ObjectFormatter.Format(value);

        Assert.AreEqual(value.ToString(), result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_DateOnly_FormatsAsScalar()
    {
        var value = new DateOnly(2024, 1, 1);

        var result = ObjectFormatter.Format(value);

        Assert.AreEqual(value.ToString(), result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_TimeOnly_FormatsAsScalar()
    {
        var value = new TimeOnly(12, 30, 0);

        var result = ObjectFormatter.Format(value);

        Assert.AreEqual(value.ToString(), result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_TimeSpan_FormatsAsScalar()
    {
        var value = new TimeSpan(
            days: 1,
            hours: 2,
            minutes: 3,
            seconds: 4);

        var result = ObjectFormatter.Format(value);

        Assert.AreEqual(value.ToString(), result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_Guid_FormatsAsScalar()
    {
        var value = Guid.Parse(
            "12345678-1234-1234-1234-123456789abc");

        var result = ObjectFormatter.Format(value);

        Assert.AreEqual(
            "12345678-1234-1234-1234-123456789abc",
            result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_NullableIntWithValue_FormatsUnderlyingValue()
    {
        int? value = 42;

        var result = ObjectFormatter.Format(value);

        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_NullableDecimalWithValue_FormatsUnderlyingValue()
    {
        decimal? value = 5.0m;

        var result = ObjectFormatter.Format(value);

        Assert.AreEqual("5.0", result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_NullableWithoutValue_FormatsNull()
    {
        int? value = null;

        var result = ObjectFormatter.Format(
            value,
            new PrintOptions
            {
                IncludeNulls = true
            });

        Assert.AreEqual("null", result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_NullWithoutIncludeNulls_ReturnsEmptyString()
    {
        object? value = null;

        var result = ObjectFormatter.Format(
            value,
            new PrintOptions
            {
                IncludeNulls = false
            });

        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_MixedValues_UsesCSharpScalarFormatting()
    {
        object?[] values =
        [
            42,
            5.0m,
            3.14d,
            true,
            null,
            "hello",
            'A'
        ];

        var result = ObjectFormatter.Format(
            values,
            new PrintOptions
            {
                IncludeNulls = true
            });

        Assert.AreEqual(
            "[42, 5.0, 3.14, true, null, \"hello\", 'A']",
            result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_NewerDotNetScalarTypes_DoNotReflectMembers()
    {
        object[] values =
        [
            (Half)1.5f,
            new DateOnly(2024, 1, 1),
            new TimeOnly(12, 30, 0)
        ];

        var result = ObjectFormatter.Format(values);

        var expected =
            $"[{values[0]}, {values[1]}, {values[2]}]";

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void ObjectFormatter_Format_CustomStruct_FormatsMembers()
    {
        var value = new TestPoint
        {
            X = 10,
            Y = 20
        };

        var result = ObjectFormatter.Format(value);

        Assert.AreEqual("{X: 10, Y: 20}", result);
    }

    private static string NormalizeNewlines(string value)
    {
        return value.Replace("\r\n", "\n");
    }

    private struct TestPoint
    {
        public int X { get; set; }

        public int Y { get; set; }
    }
}