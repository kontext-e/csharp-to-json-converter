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

        public void Analyze(MethodDeclarationSyntax methodDeclarationSyntax, MethodModel methodModel)
        {
            List<ParameterSyntax> parameterSyntaxes = methodDeclarationSyntax
                .DescendantNodes()
                .OfType<ParameterSyntax>()
                .ToList();

            foreach (ParameterSyntax parameterSyntax in parameterSyntaxes)
            {
                IdentifierNameSyntax identifierNameSyntax = parameterSyntax.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();

                if (identifierNameSyntax != null)
                {
                    ITypeSymbol typeSymbol = SemanticModel.GetSymbolInfo(identifierNameSyntax).Symbol as ITypeSymbol;
                    methodModel.Parameters.Add(typeSymbol.ToString());
                    continue;
                }
                
                PredefinedTypeSyntax predefinedTypeSyntax = parameterSyntax.DescendantNodes().OfType<PredefinedTypeSyntax>().FirstOrDefault();


                if (predefinedTypeSyntax != null)
                {
                    ITypeSymbol typeSymbol = SemanticModel.GetSymbolInfo(predefinedTypeSyntax).Symbol as ITypeSymbol;
                    methodModel.Parameters.Add(typeSymbol.ToString());
                }
            }
        }
    }
}