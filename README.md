# EnumerablePrinter

[![NuGet](https://img.shields.io/nuget/v/EnumerablePrinter.svg)](https://www.nuget.org/packages/EnumerablePrinter)
[![Build](https://github.com/YOUR_USERNAME/EnumerablePrinter/actions/workflows/build.yml/badge.svg)](https://github.com/YOUR_USERNAME/EnumerablePrinter/actions)

A lightweight C# extension that brings Python-style `print()` elegance to your `IEnumerable<T>`. Debug smarter, print cleaner.

---

## ✨ Features

- Format any `IEnumerable<T>` like `{ a, b, c }`
- Customize output with a lambda (e.g., `item => $"[{item}]"`)
- Print to `Console`, `StringWriter`, `StreamWriter`, or any `TextWriter`
- Modern C# 11/.NET 8 compatible

---

## 🚀 Installation

```bash
dotnet add package EnumerablePrinter
