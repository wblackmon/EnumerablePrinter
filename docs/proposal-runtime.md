# Proposal: Object and Collection Formatting for .NET Diagnostics

## Summary

Introduce a small diagnostic formatter for objects, dictionaries, and collections. The API provides readable, JSON-adjacent text for logging, debugging, console output, and developer tooling without requiring callers to define a serializer model.

## Motivation

Developers frequently need structured, human‑readable representations of objects for:

- Logging
- Diagnostics
- Console output
- Developer tools
- Telemetry
- Debugging

Existing options have limitations:

- object.ToString() is unstructured
- System.Text.Json is too heavy for quick inspection
- Debugger displays are not available in logs or console

A lightweight, deterministic formatter fills this gap.

## Proposed API

```csharp
namespace System.Diagnostics
{
    public static class ObjectFormatter
    {
        public static string Format(object? value, ObjectFormatOptions? options = null);
        public static void Write(object? value, TextWriter writer, ObjectFormatOptions? options = null);
    }

    public sealed class ObjectFormatOptions
    {
        public int MaxDepth { get; set; } = 50;
        public int MaxItems { get; set; } = int.MaxValue;
        public bool Pretty { get; set; } = false;
        public int IndentSize { get; set; } = 2;
        public bool IncludeNulls { get; set; } = true;
        public bool IncludePrivateFields { get; set; } = false;
        public bool IncludePrivateProperties { get; set; } = false;
    }
}

namespace System.Diagnostics.Extensions
{
    public static class PrintExtensions
    {
        public static T Print<T>(
            this T value,
            TextWriter? writer = null,
            ObjectFormatOptions? options = null);

        public static T PrintToConsole<T>(
            this T value,
            ObjectFormatOptions? options = null);
    }
}
```

The formatter is the core API. The printing methods are optional extensions in a separate namespace so applications can opt in to console and writer convenience methods explicitly.

## Formatting Rules

- Null → null
- Strings → "text"
- Primitive values → their standard text representation
- Enumerables → [ item1, item2, … ]
- Dictionaries → { key: value, … }
- Objects → { Property: Value }
- Cycles → `<Circular Reference>`
- Depth limit → `<max depth>`
- Max items → `<max items>`

## Ordering

The initial implementation follows reflection and dictionary enumeration order. Deterministic ordering would require an explicit contract and should be evaluated separately rather than claimed as an inherent property of this API.

## Safety

- Skip indexers
- Skip getters that throw
- Optional private field inclusion
- No shared mutable formatter state

## Alternatives Considered

- System.Text.Json
- DebuggerDisplay
- ToString()

## Risks

- Reflection trimming
- Reflection safety
- Performance expectations

## Implementation Notes

The existing EnumerablePrinter implementation demonstrates feasibility and provides a reference implementation. The current package API is `EnumerablePrinter.Extensions.PrintExtensions` plus `EnumerablePrinter.Abstractions.PrintOptions`; any framework proposal should be treated as a new API design rather than a direct namespace transplant.
