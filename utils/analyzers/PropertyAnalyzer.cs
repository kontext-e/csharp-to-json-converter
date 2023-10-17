using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace csharp_to_json_converter.utils.analyzers
{
    public class PropertyAnalyzer : AbstractAnalyzer
    {
        internal PropertyAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel) : base(syntaxTree, semanticModel)
        {
        }

        public void Analyze(ClassDeclarationSyntax classDeclarationSyntax, ClassModel classModel)
        {
            List<PropertyDeclarationSyntax> propertyDeclarationSyntaxes = classDeclarationSyntax
                .DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .ToList();

            foreach (PropertyDeclarationSyntax propertyDeclarationSyntax in propertyDeclarationSyntaxes)
            {
                IPropertySymbol propertySymbol = SemanticModel.GetDeclaredSymbol(propertyDeclarationSyntax) as IPropertySymbol;

                PropertyModel propertyModel = new PropertyModel();
                propertyModel.Name = propertySymbol.Name;
                propertyModel.Fqn = propertySymbol.ToString();
                propertyModel.Type = propertySymbol.Type.ToString();
                propertyModel.Sealed = propertySymbol.IsSealed;
                propertyModel.Static = propertySymbol.IsStatic;
                propertyModel.Override = propertySymbol.IsOverride;
                propertyModel.Virtual = propertySymbol.IsVirtual;
                propertyModel.Extern = propertySymbol.IsExtern;
                propertyModel.Accessibility = propertySymbol.DeclaredAccessibility.ToString();
                
                if (propertyDeclarationSyntax.AccessorList is not null)
                {
                    foreach (var AccessorType in propertyDeclarationSyntax.AccessorList.Accessors)
                    {
                        propertyModel.Accessors.Add(AccessorType.Modifiers.ToString() + " " + AccessorType.Keyword.ToString());
                    }
                }
                if (propertyDeclarationSyntax.ExpressionBody is not null)
                {
                    propertyModel.Accessors.Add("get");
                }

                classModel.Properties.Add(propertyModel);
            }
        }
    }
}
