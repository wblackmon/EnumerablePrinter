namespace EnumerablePrinter.Tests;

using EnumerablePrinter;
using EnumerablePrinter.Abstractions;
using EnumerablePrinter.Extensions;

[TestClass]
public sealed class FormatExtensionsTests
{
    [TestMethod]
    public void Format_ReturnsDiagnosticText()
    {
        var result = new[] { 1, 2, 3 }.Format();

        Assert.AreEqual("[1, 2, 3]", result);
    }

    [TestMethod]
    public void Format_UsesTheObjectFormatterAndOptions()
    {
        var value = new[] { "alpha", "beta" };
        var options = new PrintOptions
        {
            Pretty = true,
            IndentSize = 4,
            MaxDepth = 3,
            MaxItems = 10
        };

        var expected = ObjectFormatter.Format(value, options);
        var actual = value.Format(options);

        Assert.AreEqual(expected, actual);
    }
}
