using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;

namespace csharp_to_json_converter.utils.analyzers
{
    public class PropertyAnalyzer : AbstractAnalyzer
    {
        internal PropertyAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel) : base(syntaxTree, semanticModel) { }

        public void Analyze(INamedTypeSymbol namedTypeSymbol, MemberOwningModel memberOwningModel)
        {
            foreach (var member in namedTypeSymbol.GetMembers())
            {
                if (member is not IPropertySymbol property) { continue; }
                var propertyModel = FillPropertyModel(property);
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
                Readonly = propertySymbol.IsReadOnly,
                Accessibility = propertySymbol.DeclaredAccessibility.ToString().Length == 0 ? "Internal" : propertySymbol.DeclaredAccessibility.ToString()
            };
            return propertyModel;
        }
    }
}
