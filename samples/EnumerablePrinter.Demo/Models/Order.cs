namespace EnumerablePrinter.Demo.Models;
using EnumerablePrinter.Extensions;
public class Order
{
    public int Id { get; set; }
    public required Person Customer { get; set; }
    public required List<string> Items { get; set; }
}
