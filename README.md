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
- keeps the public API intentionally small and focused

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
