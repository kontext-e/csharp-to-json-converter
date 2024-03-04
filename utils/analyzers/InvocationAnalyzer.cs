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

            var callersAsync = SymbolFinder.FindCallersAsync(methodSymbol, _solution).Result;
            foreach (var caller in callersAsync)
            {
                var invokeModels = CreateInvokeModelForEachLocation(caller);
                methodModel.InvokedBy.AddRange(invokeModels);
            }
        }

        private static IEnumerable<InvocationModel> CreateInvokeModelForEachLocation(SymbolCallerInfo caller)
        {
            return caller.Locations.Select(location => new InvocationModel
            {
                MethodId = caller.CallingSymbol.ToString(), 
                LineNumber = location.GetLineSpan().StartLinePosition.Line + 1
            }).ToList();
        }

        public void ProcessImplicitObjectCreations(MethodDeclarationSyntax methodDeclarationSyntax, IMethodSymbol methodSymbol, MethodModel methodModel)
        {
            var objectCreations = methodDeclarationSyntax.DescendantNodes().OfType<ImplicitObjectCreationExpressionSyntax>().ToList();

            foreach (var objectCreation in objectCreations)
            {
                var objectCreationSymbol = SemanticModel.FindSymbolInfoOrUseSingleCandidate(objectCreation) as IMethodSymbol;

                methodModel.Invokes.Add(new InvocationModel
                {
                    MethodId = objectCreationSymbol.ToString(),
                    LineNumber = objectCreation.GetLocation().GetLineSpan().StartLinePosition.Line + 1
                });
            }
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