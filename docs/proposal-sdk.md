# Proposal: Sequence Slicing for LINQ

## Summary

Introduce Python-style slicing for `IEnumerable<T>`, complementing existing `Span<T>` and range-based indexing. The API would provide ergonomic, deferred, composable slicing for LINQ sequences.

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
    public static class Enumerable
    {
        public static IEnumerable<T> Slice<T>(
            this IEnumerable<T> source,
            int? start = null,
            int? end = null,
            int step = 1);
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
- `Skip`/`Take`, ranges, and `TakeLast`

## Risks

- Deferred execution expectations
- Hidden allocations
- Negative indexing semantics

## Scope and compatibility

The current repository implements `Slice` in the separate `EnumerablePrinter.Linq` project as `EnumerablePrinter.Linq.SequenceExtensions`. The earlier proposal included `Chunk` and `IsAlphabetical`, but those APIs were removed because they duplicate existing .NET functionality and are not part of this proposal.

Before a framework submission, the API name, namespace, negative-index semantics, allocation behavior, and interaction with existing LINQ APIs require review. A new BCL extension should also avoid creating ambiguous extension-method resolution with application libraries.

## Implementation Notes

EnumerablePrinter's slicing implementation demonstrates feasibility.
