using System;
using System.Collections.Generic;
using System.Linq;
using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace csharp_to_json_converter.utils.analyzers
{
    public class PropertyAnalyzer : AbstractAnalyzer
    {
        internal PropertyAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel) : base(syntaxTree, semanticModel) { }

        internal void Analyze(INamedTypeSymbol namedTypeSymbol, MemberOwningModel memberOwningModel)
        {
            foreach (var member in namedTypeSymbol.GetMembers())
            {
                if (member is not IPropertySymbol propertySymbol) { continue; }
                
                var propertyDeclarations = propertySymbol.DeclaringSyntaxReferences;
                // Conditional Access because auto-generated Properties have no syntax
                // Partial Properties are not (yet) allowed, so there can't be more than one declaration
                var propertyDeclaration = propertyDeclarations.FirstOrDefault()?.GetSyntax() as PropertyDeclarationSyntax;
                var propertyModel = FillPropertyModel(propertySymbol, propertyDeclaration);
                Console.WriteLine(propertyDeclaration?.Type);
                Console.WriteLine(string.Join(", ", propertyModel.Type));
                Console.WriteLine(member.ToDisplayString());
                Console.WriteLine();
                memberOwningModel.Properties.Add(propertyModel);
            }
        }

        private PropertyModel FillPropertyModel(IPropertySymbol propertySymbol, PropertyDeclarationSyntax propertyDeclaration)
        {
            var propertyModel = new PropertyModel
            {
                Name = propertySymbol.Name,
                Fqn = propertySymbol.ToString(),
                Type = AnalyzePropertyType(propertySymbol, propertyDeclaration),
                Sealed = propertySymbol.IsSealed,
                Static = propertySymbol.IsStatic,
                Override = propertySymbol.IsOverride,
                Virtual = propertySymbol.IsVirtual,
                Required = propertySymbol.IsRequired,
                Extern = propertySymbol.IsExtern,
                Readonly = propertySymbol.IsReadOnly,
                Accessibility = propertySymbol.DeclaredAccessibility.ToString().Length == 0 ? "Internal" : propertySymbol.DeclaredAccessibility.ToString()
            };
            return propertyModel;
        }

        private List<string> AnalyzePropertyType(IPropertySymbol propertySymbol, PropertyDeclarationSyntax propertyDeclaration){
            if (propertyDeclaration == null) return new List<string>();
            
            var genericNameSyntax = FindGenericNameSyntax(propertyDeclaration);

            var types = new List<string>();
            if (genericNameSyntax != null)
            {
                AnalyzeGenericTypeRecursively(genericNameSyntax, types);
            }
            else
            {
                types.Add(propertySymbol.Type.ToString());
            }
            
            return types;
        }
        
        private static GenericNameSyntax FindGenericNameSyntax(PropertyDeclarationSyntax propertyDeclaration)
        {
            var genericNameSyntax = new List<GenericNameSyntax>();
            
            //Generic Syntax can either be direct node of propertyDeclaration
            genericNameSyntax.AddRange(propertyDeclaration
                .ChildNodes()
                .OfType<GenericNameSyntax>()
                .ToList());
            
            //Or wrapped in NullableTypeSyntax
            genericNameSyntax.AddRange(propertyDeclaration
                .ChildNodes()
                .OfType<NullableTypeSyntax>()
                .FirstOrDefault()? //NullableType can only exist once as child of PropertyDeclarationSyntax 
                .ChildNodes()
                .OfType<GenericNameSyntax>()
                .ToList() ?? new List<GenericNameSyntax>());
            
            //GenericTypeSyntax (wrapped or not) can only exist once as child of PropertyDeclarationSyntax
            return genericNameSyntax.FirstOrDefault();
        }

        private void AnalyzeGenericTypeRecursively(GenericNameSyntax genericNameSyntax, List<string> types)
        {
            var symbol = SemanticModel.GetSymbolInfo(genericNameSyntax).Symbol as INamedTypeSymbol;
            types.Add(symbol?.ConstructedFrom.ToDisplayString());
        
            var typeArguments = genericNameSyntax?
                .ChildNodes()
                .OfType<TypeArgumentListSyntax>()
                .ToList();
            
            foreach (var typeArgument in typeArguments)
            {
                AnalyzeNestedGenericType(types, typeArgument);
                AnalyzeNonGenericTypes(types, typeArgument);
            }
        }

        private void AnalyzeNestedGenericType(List<string> types, TypeArgumentListSyntax typeArgument)
        {
            var nestedGenericTypes = typeArgument.ChildNodes().OfType<GenericNameSyntax>().ToList();
            if (nestedGenericTypes.Count <= 0) return;
            foreach (var genericType in nestedGenericTypes)
            {
                AnalyzeGenericTypeRecursively(genericType, types);
            }
        }

        private void AnalyzeNonGenericTypes(ICollection<string> types, TypeArgumentListSyntax typeArgument)
        {
            foreach (var identifierNameSyntax in typeArgument.ChildNodes().OfType<IdentifierNameSyntax>())
            {
                types.Add(SemanticModel.GetSymbolInfo(identifierNameSyntax).Symbol?.ToDisplayString());
            }
        
            foreach (var predefinedTypeSyntax in typeArgument.ChildNodes().OfType<PredefinedTypeSyntax>())
            {
                types.Add(SemanticModel.GetSymbolInfo(predefinedTypeSyntax).Symbol?.ToDisplayString());
            }
        }
    }
}
