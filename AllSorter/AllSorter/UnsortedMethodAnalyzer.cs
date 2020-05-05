using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AllSorter
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UnsortedMethodAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MethodSorter";
        internal static readonly LocalizableString Title = "File contains unsorted methods";
        internal static readonly LocalizableString MessageFormat = "Method unsorted";
        internal const string Category = "Sorting";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxTreeAction(AnalyseTreeForUnsortedMethods);
        }

        public static void AnalyseTreeForUnsortedMethods(SyntaxTreeAnalysisContext context)
        {
            string fileName = context.Tree.FilePath.Split('\\').Last();
            SyntaxNode syntaxNode = context.Tree.GetRoot();
            IEnumerable<SyntaxNode> descendantNodes = syntaxNode.DescendantNodes();
            var classes = descendantNodes.OfType<ClassDeclarationSyntax>();
            foreach(var aClass in classes)
            {
                var descendants = aClass.DescendantNodes();
                var methods = descendants.OfType<MethodDeclarationSyntax>().ToList();
                var sortedMethods = methods.OrderBy(x => x.Identifier.ValueText).ToList();

                for (int i = 0; i < methods.Count; i++)
                {
                    var methodDeclarationSyntax = methods[i];

                    if (methodDeclarationSyntax.ToString() != sortedMethods[i].ToString())
                    {
                        Diagnostic diagnostic = Diagnostic.Create(Rule, methodDeclarationSyntax.GetLocation(), fileName);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}
