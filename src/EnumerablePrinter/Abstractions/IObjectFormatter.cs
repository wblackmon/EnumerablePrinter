namespace EnumerablePrinter.Abstractions;

public interface IObjectFormatter
{
    string Format(object? value);
}