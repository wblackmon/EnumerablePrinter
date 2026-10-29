# Changelog

## Unreleased

### Added

- added a chainable `Format()` extension that returns diagnostic text without writing to an output destination
- added `netstandard2.0` and `net8.0` targets to the core and optional LINQ packages
- added compatibility support for nullable annotations and `init` properties on `netstandard2.0`
- added explicit scalar formatting for common .NET value types
- added scalar support for `sbyte`, `byte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `IntPtr`, and `UIntPtr`
- added scalar support for `Half`, `float`, `double`, and `decimal`
- added scalar support for `bool`, `char`, `string`, and enum values
- added scalar support for `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`, `TimeSpan`, and `Guid`
- added escaping for special characters in strings and characters
- added demo coverage for integral, floating-point, decimal, boolean, text, enum, date/time, identifier, and mixed scalar values
- added regression tests covering supported scalar types, nullable values, escaping, and user-defined structs

### Changed

- common .NET scalar values are now formatted directly instead of being expanded through reflection
- numeric values use their normal .NET representation without C# literal suffixes such as `m`, `f`, `d`, `L`, and `UL`
- boolean values now render using C#-style `true` and `false`
- strings use double quotes and characters use single quotes
- enums render using their normal value representation
- common date, time, and identifier types render using their normal .NET representation
- user-defined structs continue through structured object formatting instead of being treated as built-in scalar values
- preserved the existing public API and collection-formatting behavior across `netstandard2.0` and `net8.0`

### Fixed

- fixed `decimal` values falling through to reflection and exposing implementation details such as `{Scale: "1"}` instead of their numeric representation
- fixed built-in value types being incorrectly treated as complex objects during formatter dispatch
- fixed string and character output so special characters such as newlines, tabs, quotes, and backslashes are escaped consistently

## 2.1.0 - Configurable Print Options

- added `PrintOptions` for controlling pretty output, indentation, recursion depth, collection limits, null handling, and private-member inclusion
- added optional `PrintOptions` parameters to `Print()` and `PrintToConsole()` while preserving compact default output
- added demo coverage and regression tests for configured formatting

---

## 2.0.0 - Optional Sequence Slicing

- added the separate `EnumerablePrinter.Linq` project for sequence extensions
- added `Slice<T>(start, end, step)` for `IEnumerable<T>` values
- added positive