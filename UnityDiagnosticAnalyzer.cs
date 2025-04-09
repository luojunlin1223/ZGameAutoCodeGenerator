using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeReview
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public partial class UnityDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(_editorDescriptor, _npoiDescriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze |
                                                   GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(OnSyntaxNodeActionEditor, SyntaxKind.UsingDirective);

            context.RegisterSyntaxNodeAction(OnSyntaxNodeActionNPOI, SyntaxKind.UsingDirective);
        }
    }
}