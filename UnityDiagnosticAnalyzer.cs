using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace CodeReview
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UnityDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private DiagnosticDescriptor EditorRule
        {
            get
            {
                if (_descriptor == null)
                {
                    _descriptor = new DiagnosticDescriptor(
                        "ZG001",
                        "检测到在非Editor文件下使用了Using UnityEditor",
                        "检测到在非Editor文件下使用了Using UnityEditor",
                        "ZGame",
                        DiagnosticSeverity.Error,
                        true);
                }

                return _descriptor;
            }
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(EditorRule);

        private DiagnosticDescriptor _descriptor;

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze |
                                                   GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(OnSyntaxNodeActionEditor, SyntaxKind.UsingDirective);
        }

        private static bool IsWithinUnityEditorPreprocessorDirective(SyntaxNode root, TextSpan span)
        {
            var relevantDirectives = new Stack<DirectiveTriviaSyntax>();

            foreach (var trivia in root.DescendantTrivia())
            {
                if (!trivia.IsDirective) continue;
                if (!(trivia.GetStructure() is DirectiveTriviaSyntax directive) ||
                    directive.Span.Start > span.Start) continue;
                if (directive.Kind() == SyntaxKind.IfDirectiveTrivia ||
                    directive.Kind() == SyntaxKind.ElifDirectiveTrivia)
                {
                    relevantDirectives.Push(directive);
                }
                else if (directive.Kind() == SyntaxKind.EndIfDirectiveTrivia && relevantDirectives.Count > 0)
                {
                    relevantDirectives.Pop();
                }
                else if (directive.Kind() == SyntaxKind.ElseDirectiveTrivia && relevantDirectives.Count > 0)
                {
                    relevantDirectives.Pop();
                    relevantDirectives.Push(directive);
                }
            }

            foreach (var directive in relevantDirectives)
            {
                if (directive.Kind() != SyntaxKind.IfDirectiveTrivia &&
                    directive.Kind() != SyntaxKind.ElifDirectiveTrivia) continue;
                var condition = (directive as ConditionalDirectiveTriviaSyntax)?.Condition.ToFullString();
                if (condition != null && condition.Contains("UNITY_EDITOR"))
                {
                    return true;
                }
            }

            return false;
        }

        private void OnSyntaxNodeActionEditor(SyntaxNodeAnalysisContext context)
        {
            var usingDirective = (UsingDirectiveSyntax)context.Node;

            if (usingDirective.Name == null || usingDirective.Name.ToString() != "UnityEditor") return;
            var filePath = context.Node.SyntaxTree.FilePath;

            if (filePath.Contains("/Editor/")) return;
            var root = context.Node.SyntaxTree.GetRoot(context.CancellationToken);
            var span = usingDirective.Span;

            if (IsWithinUnityEditorPreprocessorDirective(root, span)) return;
            var diagnostic = Diagnostic.Create(EditorRule, usingDirective.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}