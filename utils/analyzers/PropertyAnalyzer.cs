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
                Type = AnalyzePropertyType(propertySymbol),
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

        private static IEnumerable<string> AnalyzePropertyType(IPropertySymbol propertySymbol)
        {
            var types = new List<string>();
            
            if (propertySymbol.Type is not INamedTypeSymbol propertyTypeSymbol) return types;

            var nullableType = propertyTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated;
            types.Add(propertyTypeSymbol.ConstructedFrom.ToDisplayString() + (nullableType ? "?" : ""));
            types.AddRange(propertyTypeSymbol.TypeArguments
                .Select(t => t.ToDisplayString() + (t.NullableAnnotation == NullableAnnotation.Annotated ? "?" : "")));
            return types;
        }
    }
}
