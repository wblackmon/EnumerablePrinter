# Background and Motivation

## Background

Developers often need to inspect a collection while debugging, logging, or
writing a quick console diagnostic. The useful operation is usually a short
pipeline: select the relevant part of a sequence, then render that result in
a readable form.

`EnumerablePrinter` provides the formatting and output portion of that
workflow:

```csharp
using EnumerablePrinter.Extensions;

values.Print();
```

The optional `EnumerablePrinter.Linq` project adds Python-style slicing for
`IEnumerable<T>` sequences. Together, the APIs support a focused diagnostic
workflow:

```csharp
using EnumerablePrinter.Extensions;
using EnumerablePrinter.Linq;

values
    .Slice(start: 1, end: 5, step: 2)
    .Print();
```

`Slice` is therefore a supporting sequence operation in the printing API,
not a replacement for general LINQ. It narrows the data set before the
formatter renders it.

## Motivation

Collection diagnostics are common, but the usual alternatives are awkward for
quick inspection:

- `ToString()` rarely exposes collection contents or object structure.
- `Skip` and `Take` express simple bounds but do not directly express negative
  indices or a step.
- Manually converting to an array or list adds noise and may eagerly enumerate
  a sequence before it is printed.
- Serializers such as `System.Text.Json` are designed for data interchange,
  not compact, human-readable diagnostics.

The combined API keeps this workflow small and discoverable:

1. `Slice` selects the relevant sequence range.
2. `Print` formats the selected values and returns the original value for
   fluent use.

This is useful for inspecting prefixes, suffixes, windows, and every nth item
without introducing temporary variables or custom formatting code.

## Design Goals

- Make common collection inspection concise.
- Support any `IEnumerable<T>`, including non-indexable sequences.
- Preserve deferred and composable sequence behavior where possible.
- Support optional `start`, `end`, and positive `step` values, including
  negative indices.
- Keep formatting concerns in `EnumerablePrinter` and sequence concerns in
  the optional `EnumerablePrinter.Linq` project.
- Produce readable output suitable for debugging, logs, and console use.

## Scope and Non-Goals

`EnumerablePrinter` is a diagnostic formatter, not a serializer or a general
purpose query framework. The API does not attempt to replace LINQ operators
such as `Where`, `Select`, `OrderBy`, `Skip`, or `Take`.

The core package remains usable without the slicing project. Applications that
only need formatting can reference `EnumerablePrinter` alone; applications
that want the combined workflow can opt into `EnumerablePrinter.Linq`.
