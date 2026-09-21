namespace EnumerablePrinter.Tests;

using EnumerablePrinter;
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

            Assert.AreEqual("[1, 2, 3]\n", _writer.ToString().Replace("\r\n", "\n"));
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

        var text = new StringWriter();
        list.Print(text);

        Assert.AreEqual("[<Circular Reference>]\n", text.ToString().Replace("\r\n", "\n"));
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
}
