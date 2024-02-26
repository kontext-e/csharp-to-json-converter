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
            if (methodSymbol == null) return;

            var callersAsync = SymbolFinder.FindCallersAsync(methodSymbol, _solution).Result;
            foreach (var caller in callersAsync)
            {
                methodModel.InvokedBy.Add(new InvocationModel { MethodId = caller.CallingSymbol.ToString() });
            }
        }

        public void ProcessPropertyAccesses(IPropertySymbol propertySymbol, PropertyModel propertyModel)
        {
            var callers = SymbolFinder.FindCallersAsync(propertySymbol, _solution).Result;
            foreach (var caller in callers)
            {
                propertyModel.Accesses.Add(new InvocationModel { MethodId = caller.CallingSymbol.ToString() } );
            }
        }
    }
}