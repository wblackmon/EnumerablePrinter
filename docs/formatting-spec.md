# EnumerablePrinter Formatting Specification

This document describes the formatting behavior provided by the current `EnumerablePrinter` implementation.

## Type handling

- `null` is rendered as `null` when `PrintOptions.IncludeNulls` is enabled.
- Strings are quoted.
- `byte[]` values are rendered as numeric collections.
- `IDictionary` values are rendered as key/value pairs.
- Other `IEnumerable` values are rendered as collections.
- Scalar values are rendered using their `ToString()` representation.
- Other objects expose public instance properties and fields according to the configured options.

## Output shape

Compact collections use square brackets:

```text
[1, 2, 3]
```

Dictionaries use braces and a colon between each key and value:

```text
{"One": 1, "Two": 2}
```

Objects use property or field names followed by a colon:

```text
{Name: "Wayne", Age: 42}
```

`Pretty = true` adds line breaks and indentation using `IndentSize`.

## Limits and cycles

- Values beyond `MaxDepth` are rendered as `<max depth>`.
- Collection entries beyond `MaxItems` are represented by `<max items>`.
- Circular references are rendered as `<Circular Reference>`.
- Indexer properties are skipped.
- Reflection getters that throw are represented as `<error>`.

Reference tracking applies to the active traversal path, so the same object may be rendered again when it appears in separate branches without forming a cycle.

## Compatibility

The core formatter and optional LINQ package target `netstandard2.0` and `net8.0`. Formatting behavior is kept the same across both target frameworks.
