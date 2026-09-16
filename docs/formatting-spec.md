# ObjectPrinter Formatting Specification

## Overview

This document defines the complete formatting rules for `System.Formatting.ObjectPrinter`.

## Goals

- Deterministic output
- Allocation-bounded traversal
- Reflection-safe behavior
- Culture-aware formatting
- Human-readable structure

---

## 1. Type Classification

### Primitive Types

- bool, byte, sbyte, short, ushort, int, uint, long, ulong
- float, double, decimal
- char
- DateTime, DateTimeOffset, TimeSpan, Guid

Formatted using the provided `IFormatProvider` when available.

### Strings

Always wrapped in quotes:

```text
"hello world"
```

### Enumerables

Formatted as:

```text
[ item1, item2, item3 ]
```

### Dictionaries

Formatted as:

```text
{ key1: value1, key2: value2 }
```

### Objects

Formatted as:

```text
{ PropertyA = ValueA, PropertyB = ValueB }
```

---

## 2. Depth Limiting

When `MaxDepth` is exceeded, nested values are replaced with:

```text
…
```

---

## 3. Cycle Detection

Cycles are represented using the cycle marker:

```text
↻
```

This prevents infinite recursion and ensures deterministic output.

---

## 4. Max Items

When `MaxItems` is exceeded during enumeration:

```text
… (truncated)
```

This ensures bounded traversal and predictable output size.

---

## 5. Ordering Rules

To guarantee deterministic output:

- Properties are sorted alphabetically
- Fields are sorted alphabetically
- Dictionary keys are sorted by their string representation

This ensures stable output across runs and environments.

---

## 6. Error Handling

ObjectPrinter must gracefully handle reflection and enumeration errors:

- Skip getters that throw exceptions
- Skip indexers
- Skip inaccessible members unless `IncludePrivateFields = true`
- Never propagate exceptions from reflection or enumeration

These rules ensure safe, predictable formatting even for complex or hostile object graphs.
