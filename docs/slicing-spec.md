# SequenceExtensions Slicing Specification

## Overview

Defines slicing semantics for `IEnumerable<T>`.

---

## 1. Parameters

### start

- `null` → beginning
- negative → offset from end

### end

- `null` → end
- negative → offset from end

### step

- Must be non-zero
- Negative step reverses traversal

---

## 2. Behavior

### Deferred Execution

Slicing must not enumerate the sequence until iteration begins.
All slicing operations remain lazy and composable.

### Negative Indexing

Negative indices are resolved by buffering only the required portion of the sequence.
This ensures slicing works on any `IEnumerable<T>` without requiring random access.

### Step Semantics

Example:

```text
Slice(0, 10, 2) → items 0, 2, 4, 6, 8
```

### Reversed Slicing

Example:

```text
Slice(10, 0, -1)
```

Reversed slicing is supported for all sequences, including non-indexable ones.

---

## 3. Edge Cases

- Empty sequences
- step = 1
- step > 1
- step < 0
- start > end
- start == end
- start or end beyond sequence length
- negative indices exceeding sequence length

All edge cases must produce deterministic, lazy, predictable results.
