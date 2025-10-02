# EnumerablePrinter

[![NuGet](https://img.shields.io/nuget/v/EnumerablePrinter.svg)](https://www.nuget.org/packages/EnumerablePrinter/1.0.1)
[![Build](https://github.com/wblackmon/EnumerablePrinter/actions/workflows/build.yml/badge.svg)](https://github.com/wblackmon/EnumerablePrinter/actions)

**Python-inspired LINQ extensions for expressive, ergonomic collection handling in C#.**

EnumerablePrinter brings the clarity and power of Python’s sequence operations to C#, with intuitive methods like `Slice`, `Print`, and `IsAlphabetical`. Designed for developer ergonomics, deferred execution, and clean diagnostics.

---

## ✨ Features

| Method             | Description                                                  |
|--------------------|--------------------------------------------------------------|
| `Print()`          | Pretty-print any `IEnumerable<T>` to console or `TextWriter` |
| `IsAlphabetical()` | Check if a sequence is sorted alphabetically                 |
| `Slice()`          | Extract a subsequence like Python’s `[start:end:step]`  

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
// Output: { }
```

---

## 🧰 Usage

Add `using EnumerablePrinter;` at the top of your file, and you're good to go.

### ➤ Basic Usage

```csharp
new[] { 1, 2, 3 }.Print();
// Output: { 1, 2, 3 }
```

### ➤ Empty Collections

```csharp
Enumerable.Empty<int>().Print();
// Output: { }
```

### ➤ With Custom Formatting

```csharp
var names = new List<string> { "Wayne", "Lucius", "Alfred" };
names.Print(n => $"[{n}]");
// Output: { [Wayne], [Lucius], [Alfred] }
```

### ➤ Redirecting Output (e.g. to logs or buffer)

```csharp
using var writer = new StringWriter();
names.Print(n => n.ToUpper(), writer);
Console.WriteLine(writer.ToString());
// Output: { WAYNE, LUCIUS, ALFRED }
```
## ➤ Slice Example

```csharp
var data = Enumerable.Range(1, 10);

data.Slice(2, 8);        // 3, 4, 5, 6, 7, 8
data.Slice(-3, null);    // 8, 9, 10
data.Slice(0, null, 2);  // 1, 3, 5, 7, 9
```
---

## 🔗 Links

- [NuGet Package](https://www.nuget.org/packages/EnumerablePrinter)
- [Source Code](https://github.com/wblackmon/EnumerablePrinter)

---

## 📝 License

Licensed under the MIT License.
