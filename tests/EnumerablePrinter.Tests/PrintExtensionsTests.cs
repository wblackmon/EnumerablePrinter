namespace EnumerablePrinter.Tests;

using EnumerablePrinter.Abstractions;
using EnumerablePrinter.Extensions;
using EnumerablePrinter.Diagnostics;

[TestClass]
[DoNotParallelize]
public class PrintExtensionsTests
{
    private StringWriter stringWriter = null!;
    private TextWriter textWriter = null!;

    // Runs before EACH test method
    [TestInitialize]
    public void TestSetup()
    {
        // Per-test setup
        // Example: create fresh instances, reset state
        stringWriter = new StringWriter();
        textWriter = Console.Out;
        Console.SetOut(stringWriter);

        // Diagnostics setup
        DebugUtility.MinimumSeverity = DebugSeverity.Info;
        DebugUtility.CategoryFilter = null;

        DebugUtility.Log("Starting test setup...", DebugSeverity.Info, "Test");
    }

    // Runs after EACH test method
    [TestCleanup]
    public void TestCleanup()
    {
        // Per-test cleanup
        // Example: dispose objects, reset static state
        Console.SetOut(textWriter);
        stringWriter.Dispose();
    }

    // Runs once AFTER all tests in this class
    [ClassCleanup]
    public static void ClassCleanup()
    {
        // One-time cleanup
        // Example: release shared resources
    }

    private string Output => stringWriter.ToString().Replace("\r\n", "\n");

    // ------------------------------
    // Example test method template
    // ------------------------------
    [TestMethod]
    public void MethodName_Condition_ExpectedBehavior()
    {
        // Arrange
        // (Set up all inputs, objects, mocks, and preconditions)

        // Act
        // (Execute the code under test)

        // Assert
        Assert.Inconclusive("Test not implemented yet.");
    }

    [TestMethod]
    public void Print_Int_WritesFormattedOutput()
    {
        int value = 123;

        value.Print();

        Assert.AreEqual("123\n", Output);
    }

    [TestMethod]
    public void Print_String_WritesQuotedString()
    {
        string value = "Hello, World!";

        value.Print();

        Assert.AreEqual("\"Hello, World!\"\n", Output);
    }

    [TestMethod]
    public void Print_List_WritesFormattedList()
    {
        var list = new List<int> { 1, 2, 3 };

        list.Print();

        Assert.AreEqual("[1, 2, 3]\n", Output);
    }

    [TestMethod]
    public void Print_EmptyList_WritesEmptyList()
    {
        var list = new List<int>();

        list.Print();

        Assert.AreEqual("[ ]\n", Output);
    }


    [TestMethod]
    public void Print_Dictionary_WritesFormattedDictionary()
    {
        var dict = new Dictionary<string, int>
        {
            { "one", 1 },
            { "two", 2 }
        };

        dict.Print();

        Assert.AreEqual("{\"one\": 1, \"two\": 2}\n", Output);
    }

    [TestMethod]
    public void Print_EmptyDictionary_WritesEmptyDictionary()
    {
        var dict = new Dictionary<string, int>();

        dict.Print();

        Assert.AreEqual("{ }\n", Output);
    }


    [TestMethod]
    public void Print_Object_WritesFormattedProperties()
    {
        var obj = new { Name = "John", Age = 30 };

        obj.Print();

        Assert.AreEqual("{Name: \"John\", Age: 30}\n", Output);
    }

    [TestMethod]
    public void Print_ByteArray_WritesFormattedByteArray()
    {
        var bytes = new byte[] { 1, 2, 3 };

        bytes.Print();

        Assert.AreEqual("[1, 2, 3]\n", Output);
    }
    [TestMethod]
    public void Print_EmptyByteArray_WritesEmptyList()
    {
        var bytes = Array.Empty<byte>();

        bytes.Print();

        Assert.AreEqual("[ ]\n", Output);
    }

    [TestMethod]
    public void Print_Enumerable_WritesFormattedEnumerable()
    {
        IEnumerable<int> values = new[] { 1, 2, 3 };

        values.Print();

        Assert.AreEqual("[1, 2, 3]\n", Output);
    }

    [TestMethod]
    public void Print_EmptyEnumerable_WritesEmptyList()
    {
        IEnumerable<int> values = Array.Empty<int>();

        values.Print();

        Assert.AreEqual("[ ]\n", Output);
    }

    [TestMethod]
    public void Print_ObjectWithNoProperties_FallsBackToToString()
    {
        var obj = new EmptyType(); // no public instance properties

        obj.Print();

        Assert.AreEqual($"{obj.ToString()}\n", Output);
    }
    private class EmptyType { }

    [TestMethod]
    public void Print_ObjectWithNullProperty_WritesNullValue()
    {
        var obj = new { Name = (string?)null };

        obj.Print();

        Assert.AreEqual("{Name: null}\n", Output);
    }

    [TestMethod]
    public void Print_ObjectWithUnsupportedTypeProperty_FormatsUsingToString()
    {
        var obj = new { Timestamp = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) };

        obj.Print();

        Assert.AreEqual("{Timestamp: \"" + obj.Timestamp.ToString() + "\"}\n", Output);
    }

    // ------------------------------
    [TestMethod]
    public void Print_ListWithSelfReference_DetectsCircularReference()
    {
        // Arrange
        var list = new List<object>();
        list.Add(list); // Add self-reference

        // Act
        list.Print();

        // Assert
        Assert.AreEqual("[<Circular Reference>]\n", Output);
    }
    // ------------------------------
    [TestMethod]
    public void Print_ObjectGraphWithCycle_DetectsCircularReference()
    {
        // Arrange
        var parent = new Node();
        var child = new Node();
        parent.Child = child;
        child.Child = parent; // Create a cycle

        // Act
        parent.Print();

        // Assert
        Assert.AreEqual("{Child: {Child: <Circular Reference>}}\n", Output);
    }

    private class Node
    {
        public Node? Child { get; set; }
    }
    // ------------------------------
    [TestMethod]
    public void Print_DictionaryWithSelfReference_DetectsCircularReference()
    {
        // Arrange
        var dict = new Dictionary<string, object>();
        dict["self"] = dict; // Add self-reference

        // Act
        dict.Print();

        // Assert
        Assert.AreEqual("{\"self\": <Circular Reference>}\n", Output);
    }

    [TestMethod]
    public void Print_OneShotEnumerable_DoesNotLoseFirstItem()
    {
        var values = new OneShotEnumerable(1, 2, 3);

        values.Print();

        Assert.AreEqual("[1, 2, 3]\n", Output);
    }

    [TestMethod]
    public void Print_ExcludingNulls_OmitsNullPropertiesAndItems()
    {
        var value = new
        {
            Name = (string?)null,
            Values = new string?[] { "present", null }
        };

        value.Print(options: new PrintOptions { IncludeNulls = false });

        Assert.AreEqual("{Values: [\"present\"]}\n", Output);
    }

    [TestMethod]
    public void Print_SkipsIndexerProperties()
    {
        var value = new IndexerObject();

        value.Print();

        Assert.AreEqual("{Name: \"value\"}\n", Output);
    }

    [TestMethod]
    public void Print_InvalidOptions_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => 1.Print(options: new PrintOptions { MaxDepth = -1 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => 1.Print(options: new PrintOptions { MaxItems = -1 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => 1.Print(options: new PrintOptions { IndentSize = -1 }));
    }

    private sealed class IndexerObject
    {
        public string Name => "value";

        public string this[int index] => index.ToString();
    }

    private sealed class OneShotEnumerable : IEnumerable<int>
    {
        private readonly IEnumerator<int> enumerator;

        public OneShotEnumerable(params int[] values)
        {
            enumerator = ((IEnumerable<int>)values).GetEnumerator();
        }

        public IEnumerator<int> GetEnumerator() => enumerator;

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

}

