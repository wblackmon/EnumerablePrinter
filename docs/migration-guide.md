# Migration Guide: EnumerablePrinter → System.Formatting.ObjectPrinter & System.Linq.SequenceExtensions

## Overview

This guide helps developers migrate from the existing EnumerablePrinter library to the new runtime + SDK modules. The new APIs provide deterministic formatting, safer reflection behavior, and Python‑style slicing for all `IEnumerable<T>` sequences.

---

## 1. Object Printing

### Legacy API (Object Printing)

```csharp
person.Print();
```

### Updated API (Object Printing)

```csharp
ObjectPrinter.Format(person);
```

### Options Mapping

| Old Option         | New Option             |
|--------------------|------------------------|
| MaxDepth           | MaxDepth               |
| MaxItems           | MaxItems               |
| SingleLine         | SingleLine             |
| IncludePrivate     | IncludePrivateFields   |

The new `ObjectPrintOptions` class preserves all existing behaviors while adding culture support, deterministic ordering, and safer reflection traversal.

---

## 2. Sequence Slicing

### Legacy API (Sequence Slicing)

```csharp
numbers.Slice(1, 5);
```

### Updated API (Sequence Slicing)

```csharp
numbers.Slice(1, 5);
```

The slicing API is preserved exactly, but now lives in the `System.Linq` namespace and is part of the official SDK module. Negative indexing, step semantics, and deferred execution remain identical.

---

## 3. Diagnostics

Existing diagnostic helpers (AsyncDebug, DebugUtility, DebugSeverity) remain compatible.
No migration is required for diagnostic code.

---

## 4. Analyzer Migration

Analyzer rules move from:

```text
EnumerablePrinter.Analyzers
```

to:

```text
System.Formatting.Analyzers
```

The rule identifiers and diagnostics remain the same, but the namespace changes to align with the new runtime module.

---

## 5. Benchmark Migration

Benchmark projects should reference:

- `System.Formatting.ObjectPrinter`
- `System.Linq.SequenceExtensions`

This ensures benchmarks measure the new deterministic formatting engine and slicing implementation.

---

## Summary

The migration path is intentionally minimal:

- Printing → rename to `ObjectPrinter.Format`
- Options → same names, same semantics
- Slicing → identical API, new namespace
- Diagnostics → unchanged
- Analyzers → new namespace
- Benchmarks → update references

The new modules provide deterministic behavior, safer reflection, and improved developer ergonomics while preserving the original EnumerablePrinter design philosophy.
