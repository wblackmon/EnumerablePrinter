using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EnumerablePrinter.Analyzers.Tests;

[TestClass]
public sealed class PrintMethodAnalyzerTests
{
    [TestMethod]
    public void ReportsPrintExtension()
    {
        var diagnostics = Analyze(@"
    using EnumerablePrinter.Extensions;

namespace EnumerablePrinter.Extensions
{
    public static class PrintExtensions
    {
        public static T Print<T>(this T value) => value;
    }
}

class Usage
{
    void M(int value) => value.Print();
}");

        Assert.AreEqual(1, diagnostics.Count(diagnostic => diagnostic.Id == "EP0001"));
    }

    [TestMethod]
    public void IgnoresOtherExtensionMethods()
    {
        var diagnostics = Analyze(@"
namespace EnumerablePrinter.Extensions
{
    public static class PrintExtensions
    {
        public static T PrintToConsole<T>(this T value) => value;
    }
}

class Usage
{
    void M(int value) => value.PrintToConsole();
}");

        Assert.AreEqual(0, diagnostics.Count(diagnostic => diagnostic.Id == "EP0001"));
    }

    private static ImmutableArray<Diagnostic> Analyze(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
        };
        var compilation = CSharpCompilation.Create(
            assemblyName: "AnalyzerTests",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        return compilation
            .WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new PrintMethodAnalyzer()))
            .GetAnalyzerDiagnosticsAsync()
            .GetAwaiter()
            .GetResult();
    }
}