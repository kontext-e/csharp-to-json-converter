using System.Linq;
using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace csharp_to_json_converter.utils.analyzers
{
    public class PropertyAnalyzer : AbstractAnalyzer
    {
        internal PropertyAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel) : base(syntaxTree, semanticModel)
        {
        }

        public void Analyze(TypeDeclarationSyntax typeDeclarationSyntax, ClassLikeModel classLikeModel)
        {
            var propertyDeclarationSyntaxes = typeDeclarationSyntax
                .DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .ToList();

            foreach (var propertyDeclaration in propertyDeclarationSyntaxes)
            {
                var propertySymbol = SemanticModel.GetDeclaredSymbol(propertyDeclaration) as IPropertySymbol;

                if (propertySymbol == null) continue;
                var propertyModel = new PropertyModel
                {
                    Name = propertySymbol.Name,
                    Fqn = propertySymbol.ToString(),
                    Type = propertySymbol.Type.ToString(),
                    Sealed = propertySymbol.IsSealed,
                    Static = propertySymbol.IsStatic,
                    Override = propertySymbol.IsOverride,
                    Virtual = propertySymbol.IsVirtual,
                    Extern = propertySymbol.IsExtern,
                    Accessibility = propertySymbol.DeclaredAccessibility.ToString().Length == 0 ? "Internal" : propertySymbol.DeclaredAccessibility.ToString()
                };

                if (propertyDeclaration.AccessorList is not null)
                {
                    foreach (var accessor in propertyDeclaration.AccessorList.Accessors)
                    {
                        propertyModel.Accessors.Add(new PropertyAccessorModel
                        {
                            Kind = accessor.Keyword.ToString(),
                            Accessibility = accessor.Modifiers.Count == 0 ? propertySymbol.DeclaredAccessibility.ToString() : accessor.Modifiers.ToString()
                        });
                    }
                }
                if (propertyDeclaration.ExpressionBody is not null)
                {
                    propertyModel.Accessors.Add(new PropertyAccessorModel
                        { 
                            Accessibility = propertyDeclaration.Modifiers.ToString(), 
                            Kind = "get"
                        });
                }

                classLikeModel.Properties.Add(propertyModel);
            }
        }
    }
}
