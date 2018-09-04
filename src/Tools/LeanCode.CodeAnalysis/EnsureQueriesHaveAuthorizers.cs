using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LeanCode.CodeAnalysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EnsureQueriesHaveAuthorizers : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "LNCD0002";

        private const string Category = "Cqrs";
        private const string Title = "Query should be authorized";
        private const string MessageFormat = @"`{0}` has no authorization attributes specified. Consider adding one or use [AllowUnauthorized] to explicitly mark no authorization";
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true);

        private const string QueryTypeName = "LeanCode.CQRS.IQuery";
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        public void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol)context.Symbol;
            if (IsQuery(type) && !type.HasAuthorizationAttribute())
            {
                var diagnostic = Diagnostic.Create(Rule, type.Locations[0], type.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool IsQuery(INamedTypeSymbol type)
        {
            return type.TypeKind != TypeKind.Interface && type.ImplementsInterfaceOrBaseClass(QueryTypeName) && !type.IsAbstract;
        }
    }
}
