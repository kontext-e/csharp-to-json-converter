using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

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

            foreach (var propertyDeclarationSyntax in propertyDeclarationSyntaxes)
            {
                var propertySymbol = SemanticModel.GetDeclaredSymbol(propertyDeclarationSyntax) as IPropertySymbol;

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
                    Accessibility = propertySymbol.DeclaredAccessibility.ToString()
                };

                if (propertyDeclarationSyntax.AccessorList is not null)
                {
                    foreach (var accessorType in propertyDeclarationSyntax.AccessorList.Accessors)
                    {
                        propertyModel.Accessors.Add(accessorType.Modifiers.ToString() + " " + accessorType.Keyword.ToString());
                    }
                }
                if (propertyDeclarationSyntax.ExpressionBody is not null)
                {
                    propertyModel.Accessors.Add("get");
                }

                classLikeModel.Properties.Add(propertyModel);
            }
        }
    }
}
