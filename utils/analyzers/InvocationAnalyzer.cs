using System.Collections.Generic;
using System.Linq;
using csharp_to_json_converter.model;
using csharp_to_json_converter.utils.ExtensionMethods;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;

namespace csharp_to_json_converter.utils.analyzers
{
    public class InvocationAnalyzer : AbstractAnalyzer
    {
        private readonly Solution _solution;

        internal InvocationAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel, Solution solution) : base(syntaxTree, semanticModel)
        {
            _solution = solution;
        }
        
        public void ProcessInvocations(IMethodSymbol methodSymbol, MethodModel methodModel)
        {
            if (methodSymbol == null || methodSymbol.IsPartialDefinition) return;

            var invocationInfos = SymbolFinder.FindCallersAsync(methodSymbol, _solution).Result;
            foreach (var invokationInfo in invocationInfos)
            {
                var invokeModels = CreateInvokeModelForEachLocation(invokationInfo);
                methodModel.InvokedBy.AddRange(invokeModels);
            }
        }

        private IEnumerable<InvocationModel> CreateInvokeModelForEachLocation(SymbolCallerInfo invocationInfo)
        {
            return invocationInfo.Locations.Select(invokationlocation => new InvocationModel
            {
                MethodId = invocationInfo.CallingSymbol.ToString(), 
                LineNumber = invokationlocation.GetLineSpan().StartLinePosition.Line + 1,
            }).ToList();
        }

        public void ProcessArrayCreations(MethodDeclarationSyntax methodDeclarationSyntax, MethodModel methodModel)
        {
            var arrayCreations = methodDeclarationSyntax.DescendantNodes().OfType<VariableDeclarationSyntax>().ToList();

            foreach (var arrayCreation in arrayCreations)
            {
                var arrayTypeSyntax = arrayCreation.ChildNodes().ToList()[0];
                if (SemanticModel.GetSymbolInfo(arrayTypeSyntax).Symbol is not IArrayTypeSymbol arrayTypeSymbol) continue;

                methodModel.CreatesArrays.Add( new ArrayCreationModel 
                {
                    Type = arrayTypeSymbol.ToString(),
                    LineNumber = arrayCreation.GetLocation().GetLineSpan().StartLinePosition.Line + 1
                });
            }
        }

    }
}