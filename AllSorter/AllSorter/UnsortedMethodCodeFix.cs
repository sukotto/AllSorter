using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AllSorter
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UnsortedMethodCodeFix)), Shared]
    public class UnsortedMethodCodeFix : CodeFixProvider
    {
        // TODO: Replace with actual diagnostic id that should trigger this fix.
        public const string DiagnosticId = "UnsortedMethodCodeFix";
        private const string title = "Sort methods";
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(AllSorterAnalyzer.DiagnosticId, AllSorterAnalyzer.MethodDiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (Diagnostic diagnostic in context.Diagnostics.Where(d => FixableDiagnosticIds.Contains(d.Id) && d.Id.Equals(AllSorterAnalyzer.MethodDiagnosticId)))
            {
                // Register a code action that will invoke the fix.
                context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => GetSortedMethodDocumentAsync(context.Document, diagnostic, c),
                    equivalenceKey: title),
                diagnostic);
            }

            await Task.FromResult(Task.CompletedTask);
        }

        private static async Task<Document> GetSortedMethodDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            try
            {
                SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
                var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                DocumentEditor documentEditor = await DocumentEditor.CreateAsync(document, cancellationToken);
                foreach (var aClass in classes)
                {
                    var descendants = aClass.DescendantNodes();
                    var methods = descendants.OfType<MethodDeclarationSyntax>().ToList();
                    var sortedMethods = methods.OrderBy(x => x.Identifier.ValueText).ToList();

                    for (int i = 0; i < methods.Count; i++)
                    {
                        var method = methods[i];
                        var sortedMethod = sortedMethods[i];
                        
                        if (method.ToString() != sortedMethod.ToString())
                        {
                            MethodDeclarationSyntax copyOfSortedMethod = SyntaxFactory.MethodDeclaration(sortedMethod.AttributeLists, sortedMethod.Modifiers, sortedMethod.ReturnType,
                                sortedMethod.ExplicitInterfaceSpecifier, sortedMethod.Identifier, sortedMethod.TypeParameterList, sortedMethod.ParameterList, sortedMethod.ConstraintClauses,
                                sortedMethod.Body, sortedMethod.SemicolonToken)
                                .WithLeadingTrivia(sortedMethod.GetLeadingTrivia())
                                .WithTrailingTrivia(sortedMethod.GetTrailingTrivia());
                            documentEditor.ReplaceNode(method, copyOfSortedMethod);
                        }
                    }
                }
                return documentEditor.GetChangedDocument();
            }
            catch (Exception e)
            {

                Console.WriteLine(e);
                return document;
            }
        }
    }
}
