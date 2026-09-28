using EnumerablePrinter;
using EnumerablePrinter.Linq;

var formatted = ObjectFormatter.Format(new[] { 1, 2, 3 });
if (formatted != "[1, 2, 3]")
    return Fail("ObjectFormatter.Format", formatted);

var sliced = new[] { 10, 20, 30, 40 }.Slice(start: 1, end: 4, step: 2).ToArray();
if (sliced.Length != 2 || sliced[0] != 20 || sliced[1] != 40)
    return Fail("SequenceExtensions.Slice", string.Join(", ", sliced));

Console.WriteLine("EnumerablePrinter net472 compatibility smoke test passed.");
return 0;

static int Fail(string operation, string actual)
{
    Console.Error.WriteLine($"{operation} returned unexpected output: {actual}");
    return 1;
}
