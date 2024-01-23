using System.Collections.Generic;
using System.Linq;
using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace csharp_to_json_converter.utils.analyzers
{
    public class FieldAnalyzer : AbstractAnalyzer
    {
        internal FieldAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel) : base(syntaxTree, semanticModel)
        {
        }

        public void Analyze(TypeDeclarationSyntax typeDeclarationSyntax, MemberOwningModel memberOwningModel)
        {
            List<FieldDeclarationSyntax> fieldDeclarationSyntaxes = typeDeclarationSyntax
                .DescendantNodes()
                .OfType<FieldDeclarationSyntax>()
                .ToList();

            foreach (FieldDeclarationSyntax fieldDeclarationSyntax in fieldDeclarationSyntaxes)
            {
                foreach (VariableDeclaratorSyntax variableDeclaratorSyntax in fieldDeclarationSyntax.Declaration.Variables)
                {
                    IFieldSymbol fieldSymbol = SemanticModel.GetDeclaredSymbol(variableDeclaratorSyntax) as IFieldSymbol;
                    if (fieldSymbol == null) { continue; }

                    var fieldModel = new FieldModel
                    {
                        Name = fieldSymbol.Name,
                        Fqn = fieldSymbol.ToString(),
                        Type = fieldSymbol.Type.ToString(),
                        Static = fieldSymbol.IsStatic,
                        Abstract = fieldSymbol.IsAbstract,
                        Sealed = fieldSymbol.IsSealed,
                        Override = fieldSymbol.IsOverride,
                        Virtual = fieldSymbol.IsVirtual,
                        Const = fieldSymbol.IsConst,
                        Volatile = fieldSymbol.IsVolatile
                    };

                    if (fieldSymbol.HasConstantValue)
                    {
                        fieldModel.ConstantValue = fieldSymbol.ConstantValue?.ToString();
                    }

                    fieldModel.Accessibility = fieldSymbol.DeclaredAccessibility.ToString();
                    memberOwningModel.Fields.Add(fieldModel);
                }
            }
        }
    }
}