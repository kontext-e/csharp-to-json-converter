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
            List<MemberAccessExpressionSyntax> memberAccessExpressionSyntaxes = syntaxNode
                .DescendantNodes()
                .OfType<MemberAccessExpressionSyntax>()
                .ToList();
            
            ProcessMemberAccesses(memberAccessExpressionSyntaxes, methodModel);
        }

        private void ProcessMemberAccesses(IEnumerable<MemberAccessExpressionSyntax> memberAccessExpressionSyntaxes,
            MethodModel methodModel)
        {
            foreach (MemberAccessExpressionSyntax memberAccessExpressionSyntax in memberAccessExpressionSyntaxes)
            {
                MemberAccessModel memberAccessModel = new MemberAccessModel
                {
                    LineNumber = memberAccessExpressionSyntax.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    MethodId = memberAccessExpressionSyntax.ToString()
                };
                methodModel.MemberAccesses.Add(memberAccessModel);
            }
        }
    }
}