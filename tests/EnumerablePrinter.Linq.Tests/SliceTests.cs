namespace EnumerablePrinter.Linq.Tests;

using EnumerablePrinter.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

[TestClass]
public sealed class SliceTests
{
    // ---------------------------------------------------------
    // INT TESTS
    // ---------------------------------------------------------

    [TestMethod]
    public void Slice_NoStartEnd_Int()
    {
        var seq = Enumerable.Range(0, 5);
        var result = seq.Slice().ToList();

        CollectionAssert.AreEqual(new List<int> { 0, 1, 2, 3, 4 }, result);
    }

    [TestMethod]
    public void Slice_StartOnly_Int()
    {
        var seq = Enumerable.Range(0, 5);
        var result = seq.Slice(start: 2).ToList();

        CollectionAssert.AreEqual(new List<int> { 2, 3, 4 }, result);
    }

    [TestMethod]
    public void Slice_EndOnly_Int()
    {
        var seq = Enumerable.Range(0, 10);
        var result = seq.Slice(end: 3).ToList();

        CollectionAssert.AreEqual(new[] { 0, 1, 2 }, result);
    }

    [TestMethod]
    public void Slice_StartAndEnd_Int()
    {
        var seq = Enumerable.Range(0, 10);
        var result = seq.Slice(start: 3, end: 7).ToList();

        CollectionAssert.AreEqual(new[] { 3, 4, 5, 6 }, result);
    }

    [TestMethod]
    public void Slice_Step2_Int()
    {
        var seq = Enumerable.Range(0, 10);
        var result = seq.Slice(start: 0, end: 10, step: 2).ToList();

        CollectionAssert.AreEqual(new[] { 0, 2, 4, 6, 8 }, result);
    }

    // ---------------------------------------------------------
    // STRING TESTS
    // ---------------------------------------------------------

    [TestMethod]
    public void Slice_StringSequence()
    {
        var seq = new[] { "a", "b", "c", "d", "e" };
        var result = seq.Slice(start: 1, end: 4).ToList();

        CollectionAssert.AreEqual(new[] { "b", "c", "d" }, result);
    }

    [TestMethod]
    public void Slice_StringNegativeStart()
    {
        var seq = new[] { "zero", "one", "two", "three" };
        var result = seq.Slice(start: -2).ToList();

        CollectionAssert.AreEqual(new[] { "two", "three" }, result);
    }

    // ---------------------------------------------------------
    // CHAR TESTS
    // ---------------------------------------------------------

    [TestMethod]
    public void Slice_CharSequence()
    {
        var seq = "abcdef".ToCharArray();
        var result = seq.Slice(start: 2, end: 5).ToList();

        CollectionAssert.AreEqual(new[] { 'c', 'd', 'e' }, result);
    }

    [TestMethod]
    public void Slice_CharStep()
    {
        var seq = "abcdef".ToCharArray();
        var result = seq.Slice(start: 0, end: 6, step: 2).ToList();

        CollectionAssert.AreEqual(new[] { 'a', 'c', 'e' }, result);
    }

    // ---------------------------------------------------------
    // CUSTOM OBJECT TESTS
    // ---------------------------------------------------------

    private sealed class Person
    {
        public required string Name { get; init; }

        public override bool Equals(object? obj) =>
            obj is Person p && p.Name == Name;

        public override int GetHashCode() => Name.GetHashCode();
    }

    [TestMethod]
    public void Slice_CustomObjects()
    {
        var seq = new[]
        {
            new Person { Name = "A" },
            new Person { Name = "B" },
            new Person { Name = "C" },
            new Person { Name = "D" }
        };

        var result = seq.Slice(start: 1, end: 3).ToList();

        CollectionAssert.AreEqual(
            new[] { new Person { Name = "B" }, new Person { Name = "C" } },
            result
        );
    }

    // ---------------------------------------------------------
    // ERROR TESTS
    // ---------------------------------------------------------

    [TestMethod]
    public void Slice_StepMustBeGreaterThanZero()
    {
        var seq = Enumerable.Range(0, 5);

        Assert.Throws<ArgumentOutOfRangeException>(() => seq.Slice(step: 0).ToList());
        Assert.Throws<ArgumentOutOfRangeException>(() => seq.Slice(step: -1).ToList());
    }

    // ---------------------------------------------------------
    // CLAMPING TESTS
    // ---------------------------------------------------------

    [TestMethod]
    public void Slice_StartTooLarge_ClampsToCount()
    {
        var seq = Enumerable.Range(0, 5);
        var result = seq.Slice(start: 999).ToList();

        CollectionAssert.AreEqual(new int[] { }, result);
    }

    [TestMethod]
    public void Slice_NegativeStartTooLarge_ClampsToZero()
    {
        var seq = Enumerable.Range(0, 5);
        var result = seq.Slice(start: -999).ToList();

        CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4 }, result);
    }

    // ---------------------------------------------------------
    // EMPTY SEQUENCE TESTS
    // ---------------------------------------------------------

    [TestMethod]
    public void Slice_EmptySequence_ReturnsEmpty()
    {
        var seq = Enumerable.Empty<int>();
        var result = seq.Slice().ToList();

        CollectionAssert.AreEqual(new int[] { }, result);
    }

    [TestMethod]
    public void Slice_EmptySequenceWithNegativeIndices_ReturnsEmpty()
    {
        var seq = Enumerable.Empty<int>();
        var result = seq.Slice(start: -3, end: -1).ToList();

        CollectionAssert.AreEqual(new int[] { }, result);
    }
}
