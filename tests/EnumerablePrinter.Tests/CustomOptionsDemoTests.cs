using EnumerablePrinter.Demo.Scenarios;

namespace EnumerablePrinter.Tests;

[TestClass]
[DoNotParallelize]
public class CustomOptionsDemoTests
{
    [TestMethod]
    public void Run_WritesExpectedCustomFormattingExamples()
    {
        using var output = new StringWriter();
        var originalOutput = Console.Out;

        try
        {
            Console.SetOut(output);

            CustomOptionsDemo.Run();

            var expected = string.Join(Environment.NewLine,
                "=== Custom Element Formatting (Manual Preprocessing) ===",
                "[\"<Wayne>\", \"<Lucius>\", \"<Alfred>\"]",
                "",
                "=== Custom Dictionary Formatting (Manual Preprocessing) ===",
                "{\"WAYNE\": \"1\", \"LUCIUS\": \"2\"}",
                "",
                "=== Custom Object Formatting (Manual Wrapper Object) ===",
                "{Display: \"Product(Keyboard:Mechanical)\"}",
                "",
                "=== Redirecting Output to a Writer ===",
                "[\"<Wayne>\", \"<Lucius>\", \"<Alfred>\"]",
                "",
                "");

            Assert.AreEqual(expected, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }
}