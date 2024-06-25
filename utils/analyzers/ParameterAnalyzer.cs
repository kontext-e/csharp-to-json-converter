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
            Types = parameterSymbol.Type.FindAllTypes(parameterSymbol.FindTypeArguments()),
            Name = parameterSymbol.Name
        };
    }
}