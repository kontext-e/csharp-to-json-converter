using System.Linq;
using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharp.CSharpExtensions;

namespace csharp_to_json_converter.utils.analyzers
{
    public class PropertyAnalyzer : AbstractAnalyzer
    {
        internal PropertyAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel) : base(syntaxTree, semanticModel)
        {
        }

        public void Analyze(TypeDeclarationSyntax typeDeclarationSyntax, MemberOwningModel memberOwningModel)
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
                    Required = propertySymbol.IsRequired,
                    Extern = propertySymbol.IsExtern,
                    Accessibility = propertySymbol.DeclaredAccessibility.ToString().Length == 0 ? "Internal" : propertySymbol.DeclaredAccessibility.ToString()
                };

                if (propertyDeclaration.AccessorList is not null)
                {
                    foreach (AccessorDeclarationSyntax accessor in propertyDeclaration.AccessorList.Accessors)
                    {
                        var methodSymbol = CSharpExtensions.GetDeclaredSymbol(SemanticModel, accessor);
                        propertyModel.Accessors.Add(new PropertyAccessorModel
                        {
                            Kind = accessor.Keyword.ToString(),
                            Accessibility = accessor.Modifiers.Count == 0 ? propertySymbol.DeclaredAccessibility.ToString() : methodSymbol.DeclaredAccessibility.ToString()
                        });
                    }
                }
                if (propertyDeclaration.ExpressionBody is not null)
                {
                    propertyModel.Accessors.Add(new PropertyAccessorModel
                        { 
                            Accessibility = propertySymbol.DeclaredAccessibility.ToString(), 
                            Kind = "get"
                        });
                }

                memberOwningModel.Properties.Add(propertyModel);
            }
        }
    }
}
