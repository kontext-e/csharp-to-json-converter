using System.Collections.Generic;
using System.Linq;
using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace csharp_to_json_converter.utils.analyzers
{
    public class InvocationAnalyzer : AbstractAnalyzer
    {
        internal InvocationAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel) : base(syntaxTree,
            semanticModel)
        {
        }

        public void Analyze(SyntaxNode syntaxNode, MethodModel methodModel)
        {
            List<InvocationExpressionSyntax> invocationExpressionSyntaxes = syntaxNode
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .ToList();

            ProcessInvocations(invocationExpressionSyntaxes, methodModel);
        }

        private void ProcessInvocations(IEnumerable<InvocationExpressionSyntax> invocationExpressionSyntaxes,
            MethodModel methodModel)
        {
            foreach (InvocationExpressionSyntax invocationExpressionSyntax in invocationExpressionSyntaxes)
            {
                IMethodSymbol methodSymbol = null;

                MemberAccessExpressionSyntax memberAccessExpressionSyntax =
                    invocationExpressionSyntax.Expression as MemberAccessExpressionSyntax;

                IdentifierNameSyntax identifierNameSyntax =
                    invocationExpressionSyntax.Expression as IdentifierNameSyntax;

                if (memberAccessExpressionSyntax != null)
                {
                    methodSymbol = SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax).Symbol as IMethodSymbol;
                }
                else if (identifierNameSyntax != null)
                {
                    methodSymbol = SemanticModel.GetSymbolInfo(identifierNameSyntax).Symbol as IMethodSymbol;
                }

                InvokesModel invokesModel = new InvokesModel
                {
                    LineNumber = invocationExpressionSyntax.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    MethodId = methodSymbol?.ToString()
                };
                methodModel.Invocations.Add(invokesModel);
            }
        }
    }
}