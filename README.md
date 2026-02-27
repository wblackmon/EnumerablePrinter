# EnumerablePrinter

[![NuGet Version](https://img.shields.io/nuget/v/EnumerablePrinter.svg)](https://www.nuget.org/packages/EnumerablePrinter)

**Python‑inspired LINQ extensions for expressive, ergonomic collection handling in C#.**

EnumerablePrinter brings the clarity and power of Python’s sequence operations to C#, with intuitive methods like `Slice`, `Print`, `Chunk`, and `IsAlphabetical`. Designed for developer ergonomics, deferred execution, and clean diagnostics.

---

## ✨ Latest Updates

- **Unified Square‑Bracket Formatting** – All collections now print using `[ ... ]` for a clean, modern, JSON‑adjacent style.
- **Object Property Printing** – Complex objects print their public properties automatically using a reflection‑based printer.
- **Dictionary Improvements** – Key/value pairs print inline with consistent bracket formatting.
- **Nested Collection Support** – Recursively prints arrays, lists, sets, and dictionaries with stable, predictable formatting.
- **Char Enumerable Special Case** – `IEnumerable<char>` prints as a string literal instead of a character list.
- **Custom Formatting** – User‑provided delegates are respected for all element types, including strings.
- **Output Redirection** – Print to any `TextWriter` (console, file, buffer, logger).
- **Chunk Support** – Split sequences into fixed‑size groups with lazy evaluation.
- **CRLF/LF Stability** – All scripts and examples normalized to LF for cross‑platform reliability.
- **Automated Versioning** – Repository scripts now auto‑increment semantic versions with cascading rollover.

---

## ✨ Features

| Method             | Description                                                  |
| ------------------ | ------------------------------------------------------------ |
| `Print()`          | Pretty‑print any `IEnumerable<T>` to console or `TextWriter` |
| `IsAlphabetical()` | Check if a sequence is sorted alphabetically                 |
| `Slice()`          | Extract a subsequence like Python’s `[start:end:step]`       |
| `Chunk()`          | Split a sequence into fixed‑size chunks                      |

---

## 🚀 Installation

```bash
dotnet add package EnumerablePrinter
```

---

## 🧪 Test

```bash
dotnet test
```

Sample expectation:

```csharp
Enumerable.Empty<int>().Print();
// Output: [ ]
```

---

## 🧰 Usage

Add:

```csharp
using EnumerablePrinter;
```

and you're ready to go.

---

### ➤ Basic Usage

```csharp
new[] { 1, 2, 3 }.Print();
// Output: [ 1, 2, 3 ]
```

---

### ➤ Empty Collections

```csharp
Enumerable.Empty<int>().Print();
// Output: [ ]
```

---

### ➤ Printing a Dictionary

```csharp
var dict = new Dictionary<string, int>
{
    ["Wayne"] = 1,
    ["Lucius"] = 2,
    ["Alfred"] = 3
};
dict.Print();
// Output:
// [ "Wayne": 1, "Lucius": 2, "Alfred": 3 ]
```

---

### ➤ Printing Nested Collections

```csharp
var nested = new List<int[]>
{
    new[] { 1, 2 },
    new[] { 3, 4 }
};

nested.Print();
// Output:
// [ [ 1, 2 ], [ 3, 4 ] ]
```

---

### ➤ Combining Dictionaries and Nested Collections

```csharp
var complex = new Dictionary<string, object>
{
    ["Numbers"] = new[] { 1, 2, 3 },
    ["Matrix"] = new List<int[]>
    {
        new[] { 1, 2 },
        new[] { 3, 4 }
    }
};
complex.Print();
// Output:
// [ "Numbers": [ 1, 2, 3 ], "Matrix": [ [ 1, 2 ], [ 3, 4 ] ] ]
```

---

### ➤ Printing Objects (Property Printing)

```csharp
var products = new[]
{
    new Product { id = "1", name = "Keyboard", description = "Mechanical" }
};

products.Print();
// Output:
// [ [ id: 1, name: Keyboard, description: Mechanical ] ]
```

---

### ➤ With Custom Formatting

```csharp
var names = new List<string> { "Wayne", "Lucius", "Alfred" };
names.Print(n => $"[{n}]");
// Output: [ [Wayne], [Lucius], [Alfred] ]
```

---

### ➤ Redirecting Output (e.g., logs or buffer)

```csharp
using var writer = new StringWriter();
names.Print(n => n.ToUpper(), writer);
Console.WriteLine(writer.ToString());
// Output: [ WAYNE, LUCIUS, ALFRED ]
```

---

### ➤ Chunk Example

```csharp
var data = Enumerable.Range(1, 10);

foreach (var chunk in data.Chunk(3))
{
    chunk.Print();
}
```

**Output:**

```
[ 1, 2, 3 ]
[ 4, 5, 6 ]
[ 7, 8, 9 ]
[ 10 ]
```

---

## ➤ IsAlphabetical Example

```csharp
var names = new[] { "Alice", "Bob", "Charlie" };
names.IsAlphabetical(); // true

var unsorted = new[] { "Charlie", "Alice", "Bob" };
unsorted.IsAlphabetical(); // false

var people = new[]
{
    new Person { Name = "Alice" },
    new Person { Name = "Bob" },
    new Person { Name = "Charlie" }
};
people.IsAlphabetical(p => p.Name); // true
```

---

## ➤ Slice Example

```csharp
var data = Enumerable.Range(1, 10);

data.Slice(2, 8);        // 3, 4, 5, 6, 7, 8
data.Slice(-3, null);    // 8, 9, 10
data.Slice(0, null, 2);  // 1, 3, 5, 7, 9
```

…it _looked_ like one code window to me, but **markdownlint sees it as illegal**.

You need **one outer code block**, and inside it **NO triple‑backticks at all**.  
Everything inside must be indented or quoted — _not fenced_.

Here is the correct, MD046‑compliant, single‑window block you can paste directly into your README:

---

```markdown
## ➤ Interface‑Based Printing (IDictionary, IEnumerable, Objects)

EnumerablePrinter automatically detects common .NET interfaces and routes them to the correct printer. This includes `IDictionary`, `IEnumerable`, nested collections, and reflection‑based objects. No casting or conversion is required.

### ➤ Printing `IDictionary<string,string>` (Generic)

    IDictionary<string, string> metadata = new Dictionary<string, string>
    {
        ["ViewedBy"] = "Wayne",
        ["environment"] = "dev"
    };

    metadata.Print();
    // Output:
    // [ "ViewedBy": Wayne, "environment": dev ]

### ➤ Printing `IDictionary` (Non‑Generic)

    IDictionary table = new Hashtable
    {
        ["Wayne"] = "Engineer",
        ["Lucius"] = "CTO"
    };

    table.Print();
    // Output:
    // [ "Wayne": Engineer, "Lucius": CTO ]

### ➤ Nested Dictionaries

    var nested = new Dictionary<string, object>
    {
        ["User"] = new Dictionary<string, string>
        {
            ["Name"] = "Wayne",
            ["Role"] = "Engineer"
        },
        ["Flags"] = new[] { true, false, true }
    };

    nested.Print();
    // Output:
    // [ "User": [ "Name": Wayne, "Role": Engineer ], "Flags": [ True, False, True ] ]

### ➤ Mixed‑Type `IEnumerable<object>`

    var mixed = new List<object?>
    {
        1,
        "Wayne",
        new[] { 2, 3 },
        new Dictionary<string,int> { ["X"] = 9 }
    };

    mixed.Print();
    // Output:
    // [ 1, "Wayne", [ 2, 3 ], [ "X": 9 ] ]

### ➤ Reflection‑Based Object Printing

    var product = new Product
    {
        id = "1",
        name = "Keyboard",
        description = "Mechanical"
    };

    product.Print();
    // Output:
    // { id: 1, name: Keyboard, description: Mechanical }

### ➤ Enumerable of Objects

    var products = new[]
    {
        new Product { id = "1", name = "Keyboard", description = "Mechanical" },
        new Product { id = "2", name = "Mouse", description = "Wireless" }
    };

    products.Print();
    // Output:
    // [ { id: 1, name: Keyboard, description: Mechanical }, { id: 2, name: Mouse, description: Wireless } ]

### ➤ Char Enumerable (Special Case)

    new[] { 'H', 'i' }.Print();
    // Output:
    // "Hi"

### ➤ Byte Array (Special Case)

    new byte[3].Print();
    // Output:
    // byte[3]

---


## 🔗 Links

- NuGet Package: https://www.nuget.org/packages/EnumerablePrinter
- Source Code: https://github.com/wblackmon/EnumerablePrinter

---

## 📝 License

Licensed under the MIT License.
```
