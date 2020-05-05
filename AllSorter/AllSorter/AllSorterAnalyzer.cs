using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AllSorter
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AllSorterAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ClassSorter";
        public const string MethodDiagnosticId = "MethodSorter";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.ClassAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.ClassAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.ClassAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Sorting";

        private static DiagnosticDescriptor ClassSortRule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(ClassSortRule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            //context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
            context.RegisterSyntaxTreeAction(AnalyseTreeForUnsortedClasses);
        }

        public static void AnalyseTreeForUnsortedClasses(SyntaxTreeAnalysisContext context)
        {
            string fileName = context.Tree.FilePath.Split('\\').Last();
            SyntaxNode syntaxNode = context.Tree.GetRoot();
            IEnumerable<SyntaxNode> descendantNodes = syntaxNode.DescendantNodes();
            List<ClassDeclarationSyntax> classes = descendantNodes.OfType<ClassDeclarationSyntax>().ToList();
            if (classes.Count == 0)
            {
                return;
            }

            List<ClassDeclarationSyntax> sortedClasses = classes.OrderBy(x => x.Identifier.ValueText).ToList();

            for(int i = 0; i < classes.Count; i++)
            {
                var classDeclarationSyntax = classes[i];
                if(classDeclarationSyntax.ToString() != sortedClasses[i].ToString())
                {
                    Diagnostic diagnostic = Diagnostic.Create(ClassSortRule, classDeclarationSyntax.GetLocation(), fileName);
                    context.ReportDiagnostic(diagnostic);
                }
            }

        }
    }
}
