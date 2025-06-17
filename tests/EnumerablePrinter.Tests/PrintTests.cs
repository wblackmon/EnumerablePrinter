namespace EnumerablePrinter.Tests;

using Xunit;
using System.Text;
using System.IO;

public class PrintTests
{
    [Fact]
    public void Print_DefaultWriter_PrintsCorrectly()
    {
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);

        new[] { 10, 20, 30 }.Print(writer: writer);

        var result = sb.ToString().Replace("\r\n", "\n");
        Assert.Equal("{ 10, 20, 30 }\n", result);
    }
}
