# EnumerablePrinter

[![NuGet](https://img.shields.io/nuget/v/EnumerablePrinter.svg)](https://www.nuget.org/packages/EnumerablePrinter)
[![Build](https://github.com/wblackmon/EnumerablePrinter/actions/workflows/build.yml/badge.svg)](https://github.com/YOUR_USERNAME/EnumerablePrinter/actions)

A lightweight C# extension that brings Python-style `print()` elegance to your `IEnumerable<T>`. Debug smarter, print cleaner.

---

## ✨ Features

- Format any `IEnumerable<T>` like `{ a, b, c }`
- Customize output with a lambda (e.g., `item => $"[{item}]"`)
- Print to `Console`, `StringWriter`, `StreamWriter`, or any `TextWriter`
- Modern C# 11/.NET 8 compatible

---

## 🚀 Test

dotnet test

```csharp
Enumerable.Empty<int>().Print();
// Output: { }

## 🚀 Installation

```bash
dotnet add package EnumerablePrinter

---

## 🧰 Usage

Add `using EnumerablePrinter;` at the top of your file, and you're good to go.

### ➤ Basic Usage

```csharp
new[] { 1, 2, 3 }.Print();
// Output: { 1, 2, 3 }

### ➤ Empty Collections

```csharp
Enumerable.Empty<int>().Print();
// Output: { }

### ➤ With Custom Formatting

```csharp
var names = new List<string> { "Wayne", "Lucius", "Alfred" };
names.Print(n => $"[{n}]");
// Output: { [Wayne], [Lucius], [Alfred] }

### ➤ Redirecting Output (e.g. to logs or buffer)

```csharp
using var writer = new StringWriter();
names.Print(n => n.ToUpper(), writer);
Console.WriteLine(writer.ToString());
// Output: { WAYNE, LUCIUS, ALFRED }





