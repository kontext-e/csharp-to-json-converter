using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
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
            List<InvocationExpressionSyntax> invocationExpressionSyntaxes = syntaxNode.
                DescendantNodes().
                OfType<InvocationExpressionSyntax>().
                ToList();
            
            ProcessInvocations(invocationExpressionSyntaxes, methodModel);
            
            List<MemberAccessExpressionSyntax> memberAccessExpressionSyntaxes = syntaxNode
                .DescendantNodes()
                .OfType<MemberAccessExpressionSyntax>()
                .ToList();

            ProcessMemberAccesses(memberAccessExpressionSyntaxes, methodModel);
        }

        private void ProcessMemberAccesses(IEnumerable<MemberAccessExpressionSyntax> memberAccessExpressionSyntaxes, MethodModel methodModel)
        {
            var invocations = methodModel.Invocations.ToList().Select(invocation => invocation.MethodId).ToList();
            
            foreach (var memberAccessExpressionSyntax in memberAccessExpressionSyntaxes)
            {
                var symbolInfo = SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax);
                var symbol = symbolInfo.Symbol?.ToString();
                
                //Subtract known Method invocations from memberAccesses. Field Accesses are implicitly filtered out in Java Part of this Plugin
                if (invocations.Contains(symbol)) continue;

                var memberAccessModel = new MemberAccessModel()
                {
                    LineNumber = memberAccessExpressionSyntax.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    MemberId = symbol
                };
                
                methodModel.MemberAccesses.Add(memberAccessModel);
            }
        }
        
        private void ProcessInvocations(IEnumerable<InvocationExpressionSyntax> memberAccessExpressionSyntaxes,
            MethodModel methodModel)
        {
            foreach (var memberAccesses in memberAccessExpressionSyntaxes)
            {
                IMethodSymbol methodSymbol = null;

                if (memberAccesses.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax) {
                    methodSymbol = SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax).Symbol as IMethodSymbol;
                } 
                else if (memberAccesses.Expression is IdentifierNameSyntax identifierNameSyntax) {
                    methodSymbol = SemanticModel.GetSymbolInfo(identifierNameSyntax).Symbol as IMethodSymbol;
                }

                var invokesModel = new InvocationModel()
                {
                    LineNumber = memberAccesses.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    MethodId = methodSymbol?.ToString()
                };
                methodModel.Invocations.Add(invokesModel);
            }
        }
    }
}