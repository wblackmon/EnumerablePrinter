# Changelog

All notable changes to this project will be documented in this file.

The format is based on Keep a Changelog  
and this project adheres to Semantic Versioning.

## [Unreleased]

## [1.4.0] - 2026-02-27

### Added

- Dynamic interface‑based printing via `Print(object)` with automatic routing for:
  - `IDictionary`
  - `IDictionary<TKey, TValue>`
  - `IEnumerable`
  - `IEnumerable<T>`
  - Reflection‑based object printing
- Full recursive printing for nested dictionaries and nested enumerables.
- Special‑case handling for:
  - `IEnumerable<char>` → prints as a string literal
  - `byte[]` → prints as a length descriptor
- New README examples demonstrating interface‑based printing.
- XML documentation comments for all public APIs.
- Expanded test suite covering:
  - Non‑generic dictionaries
  - Mixed‑type enumerables
  - Reflection fallback
  - Azure‑style metadata dictionaries
  - Nested dictionary/collection combinations

### Changed

- Unified printing behavior across all entry points (`Print<T>`, `Print(object)`, dictionary printers).
- Improved consistency of square‑bracket formatting for all collection types.
- Normalized all output to LF for cross‑platform stability.

### Fixed

- Incorrect handling of nested collections inside `IEnumerable<T>`.
- Edge case where complex objects inside lists were not printed using property reflection.
- Dictionary printing inconsistencies when using non‑generic `IDictionary`.

## [1.3.0] - 2026-01-15

### Added

- Reflection‑based property printing for complex objects.
- Improved nested collection formatting.
- Support for custom element formatting delegates.

## [1.2.0] - 2025-12-10

### Added

- `Chunk()` extension for splitting sequences into fixed‑size groups.
- `IsAlphabetical()` extension for alphabetical ordering checks.

## [1.1.0] - 2025-11-01

### Added

- Python‑style `Slice()` extension.

## [1.0.0] - 2025-10-01

### Added

- Initial release with basic `Print<T>` support.
