using System.Collections.Generic;
using System.Linq;
using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;
using IdentifierNameSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax;
using InvocationExpressionSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax;
using MemberAccessExpressionSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax;

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
            List<MemberAccessExpressionSyntax> memberAccessExpressionSyntaxes = syntaxNode
                .DescendantNodes()
                .OfType<MemberAccessExpressionSyntax>()
                .ToList();
            
            ProcessMemberAccesses(memberAccessExpressionSyntaxes, methodModel);

            List<InvocationExpressionSyntax> invocationExpressionSyntaxes = syntaxNode.
                    DescendantNodes().
                    OfType<InvocationExpressionSyntax>().
                    ToList();
            
            ProcessInvocations(invocationExpressionSyntaxes, methodModel);
        }

        private void ProcessMemberAccesses(IEnumerable<MemberAccessExpressionSyntax> memberAccessExpressionSyntaxes,
            MethodModel methodModel)
        {
            foreach (MemberAccessExpressionSyntax memberAccessExpressionSyntax in memberAccessExpressionSyntaxes)
            {
                
                var symbolInfo = SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax);
                var memberAccessModel = new MemberAccessModel
                {
                    LineNumber = memberAccessExpressionSyntax.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    MemberId = symbolInfo.Symbol?.ToString()
                };
                methodModel.MemberAccesses.Add(memberAccessModel);
            }
        }
        
        private void ProcessInvocations(IEnumerable<InvocationExpressionSyntax> memberAccessExpressionSyntaxes,
            MethodModel methodModel)
        {
            foreach (InvocationExpressionSyntax memberAccesses in memberAccessExpressionSyntaxes)
            {
                IMethodSymbol methodSymbol = null;

                MemberAccessExpressionSyntax memberAccessExpressionSyntax =
                    memberAccesses.Expression as MemberAccessExpressionSyntax;

                IdentifierNameSyntax identifierNameSyntax =
                    memberAccesses.Expression as IdentifierNameSyntax;

                if (memberAccessExpressionSyntax != null)
                {
                    methodSymbol = SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax).Symbol as IMethodSymbol;
                }
                else if (identifierNameSyntax != null)
                {
                    methodSymbol = SemanticModel.GetSymbolInfo(identifierNameSyntax).Symbol as IMethodSymbol;
                }

                InvocationModel invokesModel = new InvocationModel()
                {
                    LineNumber = memberAccesses.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    MethodId = methodSymbol?.ToString()
                };
                methodModel.Invocations.Add(invokesModel);
            }
        }
    }
}