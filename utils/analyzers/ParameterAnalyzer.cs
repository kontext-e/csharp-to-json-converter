using System.Collections.Generic;
using System.Linq;
using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace csharp_to_json_converter.utils.analyzers
{
    public class ParameterAnalyzer : AbstractAnalyzer
    {
        internal ParameterAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel) : base(syntaxTree, semanticModel)
        {
        }

        public void Analyze(SyntaxNode syntaxNode, MethodModel methodModel)
        {
            List<ParameterSyntax> parameterSyntaxes = syntaxNode
                .DescendantNodes()
                .OfType<ParameterSyntax>()
                .ToList();

            List<ParameterModel> parameterModels = CreateParameterModels(parameterSyntaxes);
            methodModel.Parameters.AddRange(parameterModels);
        }

        protected internal List<ParameterModel> CreateParameterModels(List<ParameterSyntax> parameterSyntaxes)
        {
            List<ParameterModel> parameterModels = new List<ParameterModel>();
            
            foreach (ParameterSyntax parameterSyntax in parameterSyntaxes)
            {
                var identifierNameSyntax = parameterSyntax.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();
                if (identifierNameSyntax != null)
                {
                    ITypeSymbol typeSymbol = SemanticModel.GetSymbolInfo(identifierNameSyntax).Symbol as ITypeSymbol;
                    parameterModels.Add(CreateParameterModel(parameterSyntax, typeSymbol));
                    continue;
                }

                var predefinedTypeSyntax = parameterSyntax.DescendantNodes().OfType<PredefinedTypeSyntax>().FirstOrDefault();
                if (predefinedTypeSyntax != null)
                {
                    ITypeSymbol typeSymbol = SemanticModel.GetSymbolInfo(predefinedTypeSyntax).Symbol as ITypeSymbol;
                    parameterModels.Add(CreateParameterModel(parameterSyntax, typeSymbol));
                }
            }

            return parameterModels;
        }

        private ParameterModel CreateParameterModel(ParameterSyntax parameterSyntax, ITypeSymbol typeSymbol)
        {
            return new ParameterModel
            {
                Type = typeSymbol?.ToString(),
                Name = parameterSyntax.Identifier.Text
            };
        }
    }
}