using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EnumerablePrinter.Analyzers
{
    /// <summary>
    /// Reports calls to the EnumerablePrinter <c>Print()</c> extension method.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PrintMethodAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(DiagnosticDescriptors.PrintMethodRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                return;

            var methodSymbol = context.SemanticModel.GetSymbolInfo(memberAccess).Symbol as IMethodSymbol;
            if (methodSymbol == null)
                return;

            if (methodSymbol.Name == "Print" &&
                methodSymbol.ContainingType?.Name == "PrintExtensions" &&
                methodSymbol.ContainingNamespace.ToDisplayString() == "EnumerablePrinter.Extensions")
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(DiagnosticDescriptors.PrintMethodRule, memberAccess.Name.GetLocation()));
            }
        }
    }
}
