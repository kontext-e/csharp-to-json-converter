using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;
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
                foreach (var location in caller.Locations)
                {
                    methodModel.InvokedBy.Add(new InvocationModel
                    {
                        MethodId = caller.CallingSymbol.ToString(),
                        LineNumber = location.GetLineSpan().StartLinePosition.Line + 1
                    });
                }
            }
        }
    }
}