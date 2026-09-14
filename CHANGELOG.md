# Changelog

## Major Rewrite and API Cleanup

The library has been completely rearchitected and recoded around a single responsibility: printing objects and collections clearly for debugging and diagnostics.

The previous implementation mixed printer behavior with convenience LINQ-style helpers. That broad scope was removed in favor of a leaner, more maintainable design focused on formatting and readability.

### Removed Features That Duplicated the .NET Framework

The following APIs were removed because they duplicated functionality already available in the BCL and did not belong in a library whose core purpose is printing:

- `Slice(...)`
- `Chunk(...)`
- `IsAlphabetical(...)`

#### `Slice(...)`

This API duplicated common .NET sequence operations such as `Skip(...)`, `Take(...)`, `Where(...)`, and range-based filtering. Standard LINQ already provides the idiomatic mechanism for slicing and filtering sequences.

#### `Chunk(...)`

Starting with .NET 6, the framework includes `Enumerable.Chunk(int size)`. Keeping a custom implementation would only duplicate a built-in API and create confusion for developers.

#### `IsAlphabetical(...)`

This convenience method was simply a thin wrapper over LINQ patterns like `OrderBy(...)` and `SequenceEqual(...)`. It added surface area without providing unique value.

---

## Post-Review Cleanup

Following a focused code review, the remaining polish items were addressed to make the library easier to use and easier to maintain:

- corrected the `Abstractions` namespace naming typo
- added an explicit `PrintToConsole()` API for clarity and discoverability
- streamlined the formatter’s type-dispatch logic into clearer helper methods
- added regression tests for console output and circular-reference handling

These changes keep the library aligned with its purpose as a single-purpose debug-printing utility without reintroducing redundant framework functionality.

---

### Why the Rewrite Was Necessary

This cleanup was done to:

- remove redundant APIs that already exist in .NET
- reduce maintenance and complexity
- avoid shadowing built-in framework methods
- keep the library focused on its real purpose: a clean, predictable object printer
- simplify the architecture around a single formatter and a small public surface

The project is now a focused, single-purpose dynamic printer with no redundant LINQ helper APIs.
