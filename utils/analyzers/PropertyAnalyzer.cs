using System.Collections.Generic;
using System.Linq;
using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharp.CSharpExtensions;

namespace csharp_to_json_converter.utils.analyzers
{
    public class PropertyAnalyzer : AbstractAnalyzer
    {
        internal PropertyAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel) : base(syntaxTree, semanticModel) { }

        public void Analyze(TypeDeclarationSyntax typeDeclarationSyntax, MemberOwningModel memberOwningModel)
        {
            AnalyzeGeneratedProperties(typeDeclarationSyntax, memberOwningModel);
            AnalyzeExplicitPropertyDeclarations(typeDeclarationSyntax, memberOwningModel);
        }

        private void AnalyzeGeneratedProperties(TypeDeclarationSyntax typeDeclarationSyntax, MemberOwningModel memberOwningModel)
        {
            if (typeDeclarationSyntax.Kind() != SyntaxKind.RecordDeclaration && typeDeclarationSyntax.Kind() != SyntaxKind.RecordStructDeclaration) return;
            if (!typeDeclarationSyntax.ChildNodes().OfType<ParameterListSyntax>().Any()) return;
            AddGeneratedProperties(typeDeclarationSyntax, memberOwningModel);
        }
        
        private void AddGeneratedProperties(TypeDeclarationSyntax typeDeclarationSyntax, MemberOwningModel recordClassModel)
        {            
            var propertiesShorthand = typeDeclarationSyntax.ChildNodes().OfType<ParameterListSyntax>().ToList()[0].Parameters;
            foreach (var property in propertiesShorthand)
            {
                var propertyModel = new PropertyModel
                {
                    Name = property.Identifier.ValueText,
                    Fqn = recordClassModel.Fqn + "." + property.Identifier.ValueText,
                    Type = property.Type?.ToString(),
                    Required = false,
                    Accessors = createAccessors(typeDeclarationSyntax, property)
                };
                recordClassModel.Properties.Add(propertyModel);
            }
        }

        private List<PropertyAccessorModel> createAccessors(TypeDeclarationSyntax typeDeclarationSyntax, ParameterSyntax property)
        {
            bool isReadonly = typeDeclarationSyntax.Kind() == SyntaxKind.RecordDeclaration
                              || (typeDeclarationSyntax.Kind() == SyntaxKind.RecordStructDeclaration && typeDeclarationSyntax.Modifiers.ToString().Contains("readonly"));
            
            var read = new PropertyAccessorModel { Kind = "get", Accessibility = "Public" };
            var write = new PropertyAccessorModel { Kind = isReadonly? "init" : "set", Accessibility = "Public" };
            return new List<PropertyAccessorModel> {read, write};
        }

        private void AnalyzeExplicitPropertyDeclarations(TypeDeclarationSyntax typeDeclarationSyntax, MemberOwningModel memberOwningModel)
        {
            var propertyDeclarationSyntaxes = typeDeclarationSyntax
                .DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .ToList();

            foreach (var propertyDeclaration in propertyDeclarationSyntaxes)
            {
                if (ModelExtensions.GetDeclaredSymbol(SemanticModel, propertyDeclaration) is not IPropertySymbol propertySymbol) continue;
                
                var propertyModel = FillPropertyModel(propertySymbol);
                AnalyzePropertyAccessors(propertyDeclaration, propertyModel, propertySymbol);
                AnalyzeExpressionBodiedProperties(propertyDeclaration, propertyModel, propertySymbol);

                memberOwningModel.Properties.Add(propertyModel);
            }
        }

        private static PropertyModel FillPropertyModel(IPropertySymbol propertySymbol)
        {
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
            return propertyModel;
        }

        private void AnalyzeExpressionBodiedProperties(PropertyDeclarationSyntax propertyDeclaration, PropertyModel propertyModel,
            IPropertySymbol propertySymbol)
        {
            if (propertyDeclaration.ExpressionBody is null) return;
            propertyModel.Accessors.Add(new PropertyAccessorModel
            { 
                Accessibility = propertySymbol.DeclaredAccessibility.ToString(), 
                Kind = "get"
            });
        }

        private void AnalyzePropertyAccessors(PropertyDeclarationSyntax propertyDeclaration, PropertyModel propertyModel, IPropertySymbol propertySymbol)
        {
            if (propertyDeclaration.AccessorList is null) return;
            foreach (var accessor in propertyDeclaration.AccessorList.Accessors)
            {
                var methodSymbol = CSharpExtensions.GetDeclaredSymbol(SemanticModel, accessor);
                propertyModel.Accessors.Add(new PropertyAccessorModel
                {
                    Kind = accessor.Keyword.ToString(),
                    Accessibility = accessor.Modifiers.Count == 0 ? propertySymbol.DeclaredAccessibility.ToString() : methodSymbol.DeclaredAccessibility.ToString()
                });
            }
        }
    }
}
