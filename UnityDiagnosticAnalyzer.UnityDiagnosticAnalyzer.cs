using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeReview
{
    public partial class UnityDiagnosticAnalyzer
    {
        private readonly DiagnosticDescriptor _npoiDescriptor = new DiagnosticDescriptor(
            "ZG002",
            "检测到在非Editor文件下使用了Using NPOI",
            "检测到在非Editor文件下使用了Using NPOI",
            "ZGame",
            DiagnosticSeverity.Error,
            true);

        private void OnSyntaxNodeActionNPOI(SyntaxNodeAnalysisContext context)
        {
            var usingDirective = (UsingDirectiveSyntax)context.Node;
            if (usingDirective.Name == null) return;

            var filePath = context.Node.SyntaxTree.FilePath;

            filePath = filePath.Replace("\\", "/");

            if (filePath.Contains("/Editor/"))
            {
                return;
            }

            if (usingDirective.Name.ToString().Contains("NPOI"))
            {
                var diagnostic = Diagnostic.Create(_npoiDescriptor, usingDirective.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}