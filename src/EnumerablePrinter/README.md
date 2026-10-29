# EnumerablePrinter

A lightweight, reflection-aware formatter for debugging .NET objects, collections, dictionaries, and scalar values.

EnumerablePrinter is intentionally focused on one job: printing data in a clear, predictable, human-readable format for console output, logging, and quick inspection during development. It does not try to duplicate features already provided by the .NET framework.

---

## Why this project exists

The library was deliberately narrowed to a single purpose:

- print common .NET scalar values using familiar .NET representations
- render collections and nested structures recursively
- show object properties in a debug-friendly way
- keep output readable and consistent
- avoid re-implementing framework features that already exist elsewhere

This makes the library easier to understand, easier to maintain, and better aligned with real debugging workflows.

---

## Features

- pretty-prints arrays, lists, sets, and other `IEnumerable` values
- prints `IDictionary` entries as key/value output
- reflects public instance properties for complex objects
- handles common .NET scalar types directly instead of reflecting over their internal members
- preserves normal .NET numeric formatting for integral, floating-point, and `decimal` values
- handles `string`, `char`, `byte[]`, and `IEnumerable<char>` gracefully
- quotes and escapes strings and characters
- formats booleans using C#-style `true` and `false`
- prevents circular-reference recursion from causing runaway output
- supports console output or any `TextWriter`
- supports configurable output through `PrintOptions`
- exposes `ObjectFormatter` for formatting without console output
- provides a chainable `Format()` extension that returns diagnostic text without writing it
- keeps the public API intentionally small and focused
- includes optional Python-style slicing through the separate `EnumerablePrinter.Linq` project
- includes a separate Roslyn analyzer project for flagging `Print()` calls in performance-sensitive code

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

## Supported frameworks

The core `EnumerablePrinter` package targets `netstandard2.0` and `net8.0`.

This supports .NET Framework 4.7.2 and later, .NET Core, and modern .NET applications without maintaining separate formatter implementations.

The optional `EnumerablePrinter.Linq` package targets the same frameworks. The analyzer remains a separate `netstandard2.0` project.

The repository includes a .NET Framework 4.7.2 compatibility smoke test under `tests/EnumerablePrinter.Compatibility.Tests`. Run it on Windows with:

```powershell
dotnet build tests/EnumerablePrinter.Compatibility.Tests/EnumerablePrinter