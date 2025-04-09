using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace CodeReview
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public partial class UnityDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_editorDescriptor);

        private readonly DiagnosticDescriptor _editorDescriptor = new DiagnosticDescriptor(
            "ZG001",
            "检测到在非Editor文件下使用了Using UnityEditor",
            "检测到在非Editor文件下使用了Using UnityEditor",
            "ZGame",
            DiagnosticSeverity.Error,
            true);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze |
                                                   GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(OnSyntaxNodeActionEditor, SyntaxKind.UsingDirective);
        }
    }
}