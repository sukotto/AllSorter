using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AllSorter
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AllSorterCodeFixProvider)), Shared]
    public class AllSorterCodeFixProvider : CodeFixProvider
    {
        private const string title = "Sort classes";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(AllSorterAnalyzer.DiagnosticId, AllSorterAnalyzer.MethodDiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            //var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            foreach (Diagnostic diagnostic in context.Diagnostics.Where(d => FixableDiagnosticIds.Contains(d.Id) && d.Id.Equals(AllSorterAnalyzer.DiagnosticId)))
            {

                // Register a code action that will invoke the fix.
                context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => GetSortedClassDocumentAsync(context.Document, diagnostic, c),
                    equivalenceKey: title),
                diagnostic);

            }

            await Task.FromResult(Task.CompletedTask);
        }

        private static async Task<Document> GetSortedClassDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            try
            {
                SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
                List<ClassDeclarationSyntax> classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
                List<ClassDeclarationSyntax> sortedClasses = classes.OrderBy(x => x.Identifier.ValueText).ToList();

                DocumentEditor documentEditor = await DocumentEditor.CreateAsync(document, cancellationToken);
                for (var index = 0; index < classes.Count; index++)
                {
                    ClassDeclarationSyntax aClass = classes[index];
                    ClassDeclarationSyntax aSortedClass = sortedClasses[index];

                    if (aClass.ToString() != aSortedClass.ToString())
                    {
                        /* Using aSortedClass directly in ReplaceNode would cause an error. See https://github.com/dotnet/roslyn/issues/37226 */
                        ClassDeclarationSyntax copyOfSortedClass = SyntaxFactory.ClassDeclaration(attributeLists: aSortedClass.AttributeLists,
                            modifiers: aSortedClass.Modifiers, identifier: aSortedClass.Identifier, typeParameterList: aSortedClass.TypeParameterList,
                            baseList: aSortedClass.BaseList, constraintClauses: aSortedClass.ConstraintClauses, members: aSortedClass.Members)
                            .WithLeadingTrivia(aSortedClass.GetLeadingTrivia())
                            .WithTrailingTrivia(aSortedClass.GetTrailingTrivia());

                        documentEditor.ReplaceNode(aClass, copyOfSortedClass);
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
