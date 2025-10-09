using System.Text;

namespace EnumerablePrinter.Tests
{
    public class PrintTests
    {
        private static string CaptureOutput(Action action)
        {
            var sb = new StringBuilder();
            using var writer = new StringWriter(sb);
            var originalOut = Console.Out;
            Console.SetOut(writer);
            try
            {
                action();
            }
            finally
            {
                Console.SetOut(originalOut);
            }
            return sb.ToString().Replace("\r\n", "\n");
        }

        [Fact]
        public void Print_String_PrintsQuoted()
        {
            var result = CaptureOutput(() => "hello".Print());
            Assert.Equal("\"hello\"\n", result);
        }

        [Fact]
        public void Print_CharEnumerable_PrintsAsString()
        {
            var result = CaptureOutput(() => new[] { 'a', 'b' }.Print());
            Assert.Equal("\"ab\"\n", result);
        }

        [Fact]
        public void Print_ByteArray_PrintsLength()
        {
            var result = CaptureOutput(() => new byte[5].Print());
            Assert.Equal("byte[5]\n", result);
        }

        [Fact]
        public void Print_IntArray_PrintsElements()
        {
            var result = CaptureOutput(() => new[] { 1, 2, 3 }.Print());
            Assert.Equal("{ 1, 2, 3 }\n", result);
        }

        [Fact]
        public void Print_EmptyEnumerable_PrintsEmptyBraces()
        {
            var result = CaptureOutput(() => Enumerable.Empty<int>().Print());
            Assert.Equal("{ }\n", result);
        }

        [Fact]
        public void Print_WithCustomFormatting_PrintsFormatted()
        {
            var names = new List<string> { "Wayne", "Lucius", "Alfred" };
            var result = CaptureOutput(() => names.Print(n => $"[{n}]"));
            Assert.Equal("{ [Wayne], [Lucius], [Alfred] }\n", result);
        }

        [Fact]
        public void Print_RedirectsOutputToWriter()
        {
            var names = new List<string> { "Wayne", "Lucius", "Alfred" };
            var sb = new StringBuilder();
            using var writer = new StringWriter(sb);

            names.Print(n => n.ToUpper(), writer);

            var result = sb.ToString().Replace("\r\n", "\n");
            Assert.Equal("{ WAYNE, LUCIUS, ALFRED }\n", result);
        }

        [Fact]
        public void Print_Dictionary_PrintsKeyValuePairs()
        {
            var dict = new Dictionary<string, int> { ["Wayne"] = 1, ["Alfred"] = 2 };
            var result = CaptureOutput(() => dict.Print());
            Assert.Equal("{ \"Wayne\": 1, \"Alfred\": 2 }\n", result);
        }

        [Fact]
        public void Print_NestedCollections_PrintsRecursively()
        {
            var matrix = new[] { new[] { 1, 2 }, new[] { 3, 4 } };
            var result = CaptureOutput(() => matrix.Print());
            Assert.Equal("{ { 1, 2 }, { 3, 4 } }\n", result);
        }
    }
}