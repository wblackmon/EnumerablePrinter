# EnumerablePrinter Migration Guide

## 1.x to 2.x

Version 2.x focuses on object formatting and printing. Collection helper methods that duplicated functionality already provided by the .NET framework were removed from the core package.

### Printing

The extension API remains available:

```csharp
using EnumerablePrinter.Extensions;

value.Print();
value.PrintToConsole();
```

For formatting without writing output:

```csharp
using EnumerablePrinter;

var text = ObjectFormatter.Format(value);
```

### Sequence operations

Use standard LINQ APIs for general sequence operations such as filtering, projection, skipping, taking, and ordering. Optional Python-style slicing is available from the separate `EnumerablePrinter.Linq` project:

```csharp
using EnumerablePrinter.Linq;

var result = values.Slice(start: 1, end: 5, step: 2);
```

The slicing extension is not part of the core `EnumerablePrinter` package.

### Formatting options

`PrintOptions` controls pretty output, indentation, maximum depth, maximum items, null handling, and private-member inclusion:

```csharp
using EnumerablePrinter.Abstractions;
using EnumerablePrinter.Extensions;

value.Print(options: new PrintOptions
{
    Pretty = true,
    MaxDepth = 4,
    MaxItems = 20
});
```

The default output remains compact.

### Target frameworks

Version 2.x targets `netstandard2.0` and `net8.0`. The `netstandard2.0` asset is intended for .NET Framework 4.7.2 and later, .NET Core, and other compatible runtimes.

The analyzer remains a separate `netstandard2.0` project and is not included in the core runtime package.
