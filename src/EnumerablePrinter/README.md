# EnumerablePrinter

A lightweight, reflection-aware formatter for debugging .NET objects, collections, and dictionaries.

EnumerablePrinter is intentionally focused on one job: printing data in a clear, predictable, human-readable format for console output, logging, and quick inspection during development. It does not try to duplicate features already provided by the .NET framework.

---

## Why this project exists

The library was deliberately narrowed to a single purpose:

- print common .NET values clearly
- render nested structures recursively
- show object properties in a debug-friendly way
- keep output readable and consistent
- avoid re-implementing framework features that already exist elsewhere

This makes the library easier to understand, easier to maintain, and better aligned with real debugging workflows.

---

## Features

- pretty-prints arrays, lists, sets, and other `IEnumerable` values
- prints `IDictionary` entries as key/value output
- reflects public instance properties for complex objects
- handles `string`, `byte[]`, and `IEnumerable<char>` gracefully
- prevents circular-reference recursion from causing runaway output
- supports console output or any `TextWriter`
- supports configurable output through `PrintOptions`
- exposes `ObjectFormatter` for formatting without console output
- keeps the public API intentionally small and focused
- includes optional Python-style slicing through the separate `EnumerablePrinter.Linq` project

## Optional sequence slicing

The `EnumerablePrinter.Linq` project adds a focused `Slice()` extension for `IEnumerable<T>` values. It supports optional `start`, `end`, and positive `step` parameters, including negative indices.

Reference the project alongside `EnumerablePrinter`:

```xml
<ProjectReference Include="..\..\src\EnumerablePrinter.Linq\EnumerablePrinter.Linq.csproj" />
```

Then use it with the printer:

```csharp
using EnumerablePrinter.Extensions;
using EnumerablePrinter.Linq;

var numbers = new[] { 10, 20, 30, 40, 50, 60 };

numbers.Slice(start: 1, end: 4).Print();
// [20, 30, 40]

numbers.Slice(step: 2).Print();
// [10, 30, 50]

var fruits = new[] { "Apple", "Banana", "Cherry", "Date", "Elderberry" };
fruits.Slice(start: -3).Print();
// ["Cherry", "Date", "Elderberry"]
```

Positive-index slices stream from the source; slices using negative indices buffer the source so they can resolve positions from the end. `step` must be greater than zero.

---

## Installation

```bash
dotnet add package EnumerablePrinter
```

---

## Basic usage

```csharp
using EnumerablePrinter.Extensions;

var numbers = new[] { 1, 2, 3, 4, 5 };
numbers.Print();
// [1, 2, 3, 4, 5]

var person = new { Name = "Wayne", Age = 42 };
person.Print();
// {Name: "Wayne", Age: 42}

var metadata = new Dictionary<string, int>
{
    ["One"] = 1,
    ["Two"] = 2
};
metadata.Print();
// {"One": 1, "Two": 2}
```

### Explicit console output

```csharp
new[] { 1, 2, 3 }.PrintToConsole();
```

This follows the same formatting path as `Print()`, but makes the console intent explicit.

### Formatting without output

Use `ObjectFormatter` when the formatted text should be returned or written by the caller:

```csharp
using EnumerablePrinter;

var text = ObjectFormatter.Format(new[] { 1, 2, 3 });
// [1, 2, 3]
```

### Configuring output

Use `PrintOptions` when you need limits or readable multiline output. The default remains compact output for compatibility.

```csharp
using EnumerablePrinter.Abstractions;
using EnumerablePrinter.Extensions;

var report = new
{
    Title = "Inventory",
    Items = new[] { "Keyboard", "Mouse", "Monitor" }
};

var options = new PrintOptions
{
    Pretty = true,
    IndentSize = 4,
    MaxItems = 10
};

report.Print(options: options);
```

`PrintOptions` also supports maximum recursion depth, optional null and private-member handling, and collection item limits. Pass the same options to `PrintToConsole(options)` when writing directly to the console.

---

## Output conventions

The formatter uses a consistent debug-printing style:

- collections render as `[ ... ]`
- dictionaries render as `{ "key": value }`
- object properties render as `PropertyName: value`
- strings are quoted
- `null` renders as `null`
- circular references render as `<Circular Reference>`

---

## Repository workflow

The project is organized around a clean release and publish split:

- release scripts handle versioning and git tagging
- deploy scripts handle build, test, packaging, and NuGet publishing

See the script guide in [scripts/README.md](scripts/README.md) for the full flow and dry-run usage.

---

## Final project direction

The current design intentionally avoids adding collection helpers or framework duplicates. The library remains focused on the single, practical task it does well: iterating over values and printing them in a clear, debug-friendly form.

---

## License

MIT

---

## Links

- NuGet: [EnumerablePrinter on NuGet](https://www.nuget.org/packages/EnumerablePrinter)
- Repository: [EnumerablePrinter on GitHub](https://github.com/wblackmon/EnumerablePrinter)
