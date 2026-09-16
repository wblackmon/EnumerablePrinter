# Proposal: System.Formatting.ObjectPrinter — Deterministic Object & Collection Formatting

## Summary

Introduce a deterministic, reflection‑safe, allocation‑bounded formatting subsystem for objects, dictionaries, and collections. This API provides a readable, JSON‑adjacent diagnostic format suitable for logging, debugging, console output, and developer tooling.

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
namespace System.Formatting
{
    public static class ObjectPrinter
    {
        public static string Format(object? value, ObjectPrintOptions? options = null);
        public static void Write(object? value, TextWriter writer, ObjectPrintOptions? options = null);
    }

    public sealed class ObjectPrintOptions
    {
        public int MaxDepth { get; set; } = 5;
        public int MaxItems { get; set; } = 256;
        public bool SingleLine { get; set; } = false;
        public bool IncludePrivateFields { get; set; } = false;
        public bool UseIndentedFormatting { get; set; } = true;
        public IFormatProvider? FormatProvider { get; set; }
    }
}
```

## Formatting Rules

- Null → null
- Strings → "text"
- Primitives → culture‑aware formatting
- Enumerables → [ item1, item2, … ]
- Dictionaries → { key: value, … }
- Objects → { Property = Value, Field = Value }
- Cycles → ↻
- Depth limit → …
- Max items → …

## Deterministic Ordering

- Properties sorted alphabetically
- Fields sorted alphabetically
- Dictionary keys sorted by string representation

## Safety

- Skip indexers
- Skip getters that throw
- Optional private field inclusion
- No shared mutable state
- Allocation‑bounded traversal

## Alternatives Considered

- System.Text.Json
- DebuggerDisplay
- ToString()

## Risks

- Reflection trimming
- Reflection safety
- Performance expectations

## Implementation Notes

The existing EnumerablePrinter implementation demonstrates feasibility and developer demand.
