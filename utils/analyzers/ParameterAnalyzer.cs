using System.Collections.Generic;
using System.Linq;
using csharp_to_json_converter.model;
using csharp_to_json_converter.utils.ExtensionMethods;
using Microsoft.CodeAnalysis;

namespace csharp_to_json_converter.utils.analyzers;

public class ParameterAnalyzer : AbstractAnalyzer
{
    internal ParameterAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel) : base(syntaxTree, semanticModel) { }
    
    public void Analyze(IMethodSymbol methodSymbol, MethodModel methodModel)
    {
        var parameterModels = CreateParameterModels(methodSymbol.Parameters);
        methodModel.Parameters.AddRange(parameterModels);
    }

    private static List<ParameterModel> CreateParameterModels(IEnumerable<IParameterSymbol> parameterSyntaxes)
    {
        return parameterSyntaxes.Select(CreateParameterModel).ToList();
    }

    private static ParameterModel CreateParameterModel(IParameterSymbol parameterSymbol)
    {
        return new ParameterModel
        {
            Type = FindAllTypesRecursively(parameterSymbol),
            Name = parameterSymbol.Name
        };
    }
    
    private static IEnumerable<string> FindAllTypesRecursively(IParameterSymbol parameterSymbol)
    {
        switch (parameterSymbol.Type)
        {
            case INamedTypeSymbol namedTypeSymbol:
                return namedTypeSymbol.GetAllTypes(parameterSymbol);
            case ITypeParameterSymbol typeParameterSymbol:
                return typeParameterSymbol.GetAllTypes(parameterSymbol);
            case IArrayTypeSymbol arrayTypeSymbol:
                return arrayTypeSymbol.GetAllTypes(parameterSymbol);
            default:
                return new List<string>();
        }
    }
}