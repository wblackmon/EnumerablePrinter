# Proposal: System.Linq.SequenceExtensions — Slicing & Sequence Utilities

## Summary

Introduce Python-style slicing and related sequence utilities for `IEnumerable<T>`, complementing existing `Span<T>`/`Range` slicing. Enables ergonomic, deferred, composable slicing for all LINQ sequences.

## Motivation

LINQ lacks slicing support. Developers frequently need:

- Negative indexing
- Step semantics
- Deferred slicing
- Expressive subsequence extraction

Current alternatives (`Skip`, `Take`) are not expressive enough.

## Proposed API

```csharp
namespace System.Linq
{
    public static class SequenceExtensions
    {
        public static IEnumerable<T> Slice<T>(
            this IEnumerable<T> source,
            int? start = null,
            int? end = null,
            int step = 1);

        public static bool IsAlphabetical(this IEnumerable<char> source);

        public static IEnumerable<IEnumerable<T>> Chunk<T>(
            this IEnumerable<T> source,
            int size);
    }
}
```

## Slicing Semantics

- Deferred execution
- Negative indexing supported
- Step semantics supported
- Works on any `IEnumerable<T>`
- Does not require contiguous memory

## Examples

```csharp
var slice = numbers.Slice(1, 5);
var everyOther = numbers.Slice(null, null, 2);
var lastThree = numbers.Slice(-3, null);
```

## Alternatives Considered

- Range
- `Span<T>`
- `Skip`/`Take`

## Risks

- Deferred execution expectations
- Hidden allocations
- Negative indexing semantics

## Implementation Notes

EnumerablePrinter's slicing implementation demonstrates feasibility.
