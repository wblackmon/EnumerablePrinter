# API Proposal: Chainable Diagnostic Formatting and Printing

## Summary

Provide a small, composable API for formatting and inspecting values during
debugging, logging, and console development workflows. The proposal adds an
instance-style `Format` extension that returns diagnostic text, alongside the
existing `ObjectFormatter.Format` entry point and `Print` output extensions.

The API has two cooperating parts:

- `Format` converts a value to readable diagnostic text and returns a `string`
  without writing output.
- `Print` formats a value, writes one line to a `TextWriter` or the console,
  and returns the original value.

The optional `EnumerablePrinter.Linq` package adds `Slice` for selecting a
range of an `IEnumerable<T>` before it is printed.

## Scenarios

### Format a value without output

```csharp
using EnumerablePrinter.Extensions;

var text = value.Format();
```

The returned string can be passed to a logger, written to a response or view
writer, or further composed using normal string APIs:

```csharp
var diagnosticText = request.Format(options);
logger.LogError(exception, "Request details: {Details}", diagnosticText);
```

`Format()` already returns a `string`, so calling `.ToString()` on its result
does not add value.

### Print a value

```csharp
using EnumerablePrinter.Extensions;

value.Print();
```

### Print to a specific writer

```csharp
using EnumerablePrinter.Extensions;

value.Print(writer, options);
```

### Slice and print a sequence

```csharp
using EnumerablePrinter.Extensions;
using EnumerablePrinter.Linq;

values
    .Slice(start: 1, end: 5, step: 2)
    .Print();
```

Slicing supports the printing workflow by selecting which elements to inspect.
The resulting sequence can also be formatted without writing it:

```csharp
var text = values
    .Slice(start: 1, end: 5, step: 2)
    .Format(options);
```

## Proposed API

```csharp
namespace EnumerablePrinter;

public static class ObjectFormatter
{
  public static string Format(object? value, PrintOptions? options = null);

    public static void Write(
        object? value,
        TextWriter writer,
        PrintOptions? options = null);
}

namespace EnumerablePrinter.Extensions;

public static class FormatExtensions
{
  public static string Format<T>(
    this T value,
    PrintOptions? options = null);
}

public static class PrintExtensions
{
    public static T Print<T>(
        this T value,
        TextWriter? writer = null,
        PrintOptions? options = null);

    public static T PrintToConsole<T>(
        this T value,
        PrintOptions? options = null);
}

namespace EnumerablePrinter.Linq;

public static class SequenceExtensions
{
    public static IEnumerable<T> Slice<T>(
        this IEnumerable<T> source,
        int? start = null,
        int? end = null,
        int step = 1);
}
```

`Format` delegates to the same formatting behavior as `ObjectFormatter.Format`.
`PrintOptions` controls pretty output, indentation, maximum depth, maximum item
count, null handling, and private-member handling.

## Semantics

### Formatting

- `null` is rendered as `null`.
- Strings are quoted.
- Enumerables are rendered as collections.
- Dictionaries are rendered as key/value pairs.
- Objects expose configured public properties and fields.
- Circular references, depth limits, and item limits use explicit diagnostic
  markers.

`value.Format(options)` and `ObjectFormatter.Format(value, options)` return text
without writing output. `ObjectFormatter.Write` writes formatted text without
appending a newline.

### Printing

`Print` and `PrintToConsole` format the value, write one line, and return the
same value that was passed in. Returning `T` is intentional: it makes the
common diagnostic operation observable without forcing a temporary variable.

```csharp
var result = values.Print();
Assert.AreSame(values, result);
```

The return behavior must be called out in documentation because a method named
`Print` could reasonably be expected to return `void`. Callers that do not need
the return value can simply ignore it.

`Format` and `Print` are complementary endpoints, not a promise that
`value.Format().Print()` preserves the original value. `Format` returns a
string; passing that string to the existing generic `Print` formats it as a
string value, which may add string quoting. Callers should write or log the
formatted string directly. `value.Print()` remains the concise operation when
the intent is to format and immediately write the original value.

### Slicing

`Slice` returns a lazily evaluated `IEnumerable<T>`:

- `start` is inclusive and defaults to the beginning.
- `end` is exclusive and defaults to the end.
- Negative `start` and `end` values count from the end.
- Indices are clamped to the sequence bounds.
- `step` must be greater than zero.
- Negative-index slices may buffer the source because an arbitrary
  `IEnumerable<T>` does not provide a count or random access.

The current API does not support negative steps or reverse traversal. Reverse
slicing should be proposed separately after its semantics and allocation
behavior are defined.

## Design Rationale

The API separates pure formatting from output side effects. Consumers that need
a string can use `value.Format(options)` or `ObjectFormatter.Format`; consumers
that want immediate diagnostic output can use `Print`; and consumers that need
a custom destination can use `ObjectFormatter.Write` or `Print` with a writer.

The extension methods are intentionally small and scenario-driven. The common
case does not require constructing a formatter object, while `PrintOptions`
provides configuration for callers that need more control.

`Slice` remains in a separate package because it is a sequence operation rather
than a formatting requirement. The core formatter package therefore remains
useful without adding LINQ-specific functionality.

## Alternatives Considered

- `ToString()` does not reliably expose collection contents or object structure.
- A `Format()` extension returning `string` can be used with ordinary string,
  logging, and writer APIs without introducing a custom formatted-value type.
- `System.Text.Json` is intended for serialization and is more configuration
  than is needed for quick diagnostics.
- `Skip` and `Take` handle simple ranges but do not directly express negative
  indices or stepped ranges.
- A `void Print(...)` method would avoid ambiguity about the return value but
  would make `.Slice(...).Print()` less composable in diagnostic pipelines.

## Compatibility and Scope

The core and LINQ packages target `netstandard2.0` and `net8.0`. The analyzer is
maintained as a separate package and is outside this API proposal.

This proposal describes the `EnumerablePrinter` library API. It is not a request
to add these members to the .NET Base Class Library or to place them in the
`System` namespace.
