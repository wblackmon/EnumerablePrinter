using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using EnumerablePrinter;
using EnumerablePrinter.Abstractions;

BenchmarkRunner.Run<FormatterBenchmarks>();

[MemoryDiagnoser]
public class FormatterBenchmarks
{
	private readonly int[] values = Enumerable.Range(0, 100).ToArray();
	private readonly object nested = new
	{
		Name = "Inventory",
		Values = Enumerable.Range(0, 25).ToArray()
	};

	private readonly PrintOptions prettyOptions = new()
	{
		Pretty = true,
		IndentSize = 2
	};

	[Benchmark]
	public string FormatCompactCollection() => ObjectFormatter.Format(values);

	[Benchmark]
	public string FormatPrettyNestedObject() => ObjectFormatter.Format(nested, prettyOptions);
}
