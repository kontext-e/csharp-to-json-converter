using System.Collections.Generic;
using System.Linq;
using csharp_to_json_converter.model;
using csharp_to_json_converter.utils.ExtensionMethods;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Text;

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
                TypeArguments = AnalyzeTypeArguments(invocationInfo, invokationlocation)
            }).ToList();
        }

        private List<string> AnalyzeTypeArguments(SymbolCallerInfo invocationInfo, Location invocationLocation)
        {
            var invocationSyntax = LookupInvocationSyntaxInSourceCode(invocationInfo, invocationLocation);
            if (invocationSyntax == null)  return []; //Source likely generated (e.g. Property Accessor)
            
            var semanticModel = Analyzer.FindSemanticModelForFileContainingSyntaxNode(invocationSyntax);
            var invocationSymbol = (IMethodSymbol)semanticModel.GetSymbolInfo(invocationSyntax).Symbol;

            var allTypesPerTypeArgument = invocationSymbol?.TypeArguments.Select(symbol =>
                symbol.FindAllTypes(invocationInfo.CallingSymbol.FindTypeArguments())) ?? [];

            return allTypesPerTypeArgument.SelectMany(x=>x).ToList();
        }

        private static InvocationExpressionSyntax LookupInvocationSyntaxInSourceCode(SymbolCallerInfo invocationInfo, Location invocationLocation)
        {
            var locationOfCallingMethod = FindLocationOfCallingMethod(invocationInfo);
            var invocationExpression = FindListOfInvocationsInCallingMethod(invocationLocation, locationOfCallingMethod);

            var invocationSyntaxes = invocationExpression.Where(invocation => invocation
                .DescendantTokens()
                .Any(token => token.FullSpan.Equals(invocationLocation.SourceSpan))
            ).ToList();

            if (invocationSyntaxes.Count == 0) return null;
            
            //In case of nested Invocations, all are contained in List "invocationSyntaxes" --> take innermost invocation
            var minimumLengthSyntax = invocationSyntaxes.Min(syntax => syntax.FullSpan.Length);
            return invocationSyntaxes.First(syntax => syntax.FullSpan.Length == minimumLengthSyntax);
        }

        private static TextSpan FindLocationOfCallingMethod(SymbolCallerInfo caller)
        {
            //Even for partial methods there is only one location
            //which corresponds to the implementation of the method
            return caller.CallingSymbol.Locations[0].SourceSpan;
        }

        private static List<InvocationExpressionSyntax> FindListOfInvocationsInCallingMethod(Location invocationLocation, TextSpan locationOfCallingMethod)
        {
            if (invocationLocation.SourceTree == null) return [];

            return invocationLocation.SourceTree
                .GetRoot()
                .FindNode(locationOfCallingMethod)
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .ToList();
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