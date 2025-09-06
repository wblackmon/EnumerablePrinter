using EnumerablePrinter;
using System;
using System.Collections.Generic;
using Xunit;

public class IsAlphabeticalTests
{
    public class Item
    {
        public string Title { get; set; } = string.Empty;
    }
    [Fact]
    public void EmptySequence_ReturnsTrue()
    {
        var items = new List<Item>();
        Assert.True(items.IsAlphabetical(i => i.Title));
    }

    [Fact]
    public void SingleElement_ReturnsTrue()
    {
        var items = new List<Item> { new Item { Title = "Alpha" } };
        Assert.True(items.IsAlphabetical(i => i.Title));
    }

    [Fact]
    public void SortedSequence_ReturnsTrue()
    {
        var items = new List<Item>
        {
            new Item { Title = "Alpha" },
            new Item { Title = "Beta" },
            new Item { Title = "Gamma" }
        };
        Assert.True(items.IsAlphabetical(i => i.Title));
    }

    [Fact]
    public void UnsortedSequence_ReturnsFalse()
    {
        var items = new List<Item>
        {
            new Item { Title = "Gamma" },
            new Item { Title = "Alpha" },
            new Item { Title = "Beta" }
        };
        Assert.False(items.IsAlphabetical(i => i.Title));
    }

    [Fact]
    public void DuplicateTitles_ReturnsTrue()
    {
        var items = new List<Item>
        {
            new Item { Title = "Alpha" },
            new Item { Title = "Alpha" },
            new Item { Title = "Beta" }
        };
        Assert.True(items.IsAlphabetical(i => i.Title));
    }

    [Fact]
    public void NullSource_ThrowsArgumentNullException()
    {
        List<Item>? items = null;
        Assert.Throws<ArgumentNullException>(() =>
            items!.IsAlphabetical(i => i.Title));
    }

    [Fact]
    public void NullSelector_ThrowsArgumentNullException()
    {
        var items = new List<Item> { new Item { Title = "Alpha" } };
        Assert.Throws<ArgumentNullException>(() =>
            items.IsAlphabetical(null!));
    }

    [Fact]
    public void CaseInsensitiveComparer_ReturnsTrue()
    {
        var items = new List<Item>
        {
            new Item { Title = "alpha" },
            new Item { Title = "Beta" },
            new Item { Title = "gamma" }
        };
        Assert.True(items.IsAlphabetical(i => i.Title, StringComparer.OrdinalIgnoreCase));
    }

    [Fact]
    public void CaseSensitiveComparer_ReturnsFalse()
    {
        var items = new List<Item>
        {
            new Item { Title = "Alpha" },
            new Item { Title = "beta" },
            new Item { Title = "Gamma" }
        };
        Assert.False(items.IsAlphabetical(i => i.Title, StringComparer.Ordinal));
    }
}
