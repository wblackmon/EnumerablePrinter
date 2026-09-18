using Microsoft.CodeAnalysis;

namespace EnumerablePrinter.Analyzers
{
    public static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor PrintMethodRule =
            new DiagnosticDescriptor(
                id: "EP0001",
                title: "Avoid using Print() in performance-sensitive code",
                messageFormat: "Consider using explicit formatting or logging instead of Print()",
                category: "Usage",
                defaultSeverity: DiagnosticSeverity.Info,
                isEnabledByDefault: true,
                description: "Print() is a convenience API and may be inappropriate for performance-sensitive or large object scenarios.");
    }
}
