using System.Collections.Generic;
using System.Linq;
using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NLog;

namespace csharp_to_json_converter.utils.analyzers
{
    public class InvocationAnalyzer : AbstractAnalyzer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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

        private void ProcessInvocations(List<InvocationExpressionSyntax> invocationExpressionSyntaxes, MethodModel methodModel)
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

                if (methodSymbol == null)
                {
                    Logger.Warn("Unknown invocation expression.");
                    return;
                }

                InvokesModel invokesModel = new InvokesModel();
                invokesModel.LineNumber =
                    invocationExpressionSyntax.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                invokesModel.MethodId = methodSymbol.ToString();
                methodModel.Invocations.Add(invokesModel);
            }
        }
    }
}