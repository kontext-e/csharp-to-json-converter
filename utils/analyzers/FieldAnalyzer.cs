using System.Collections.Generic;
using System.Linq;
using csharp_to_json_converter.model;
using csharp_to_json_converter.utils.ExtensionMethods;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

            foreach (var fieldDeclarationSyntax in fieldDeclarationSyntaxes)
            {
                foreach (var variableDeclaratorSyntax in fieldDeclarationSyntax.Declaration.Variables)
                {
                    if (ModelExtensions.GetDeclaredSymbol(SemanticModel, variableDeclaratorSyntax) is not IFieldSymbol fieldSymbol) { continue; }

                    var fieldModel = new FieldModel
                    {
                        Name = fieldSymbol.Name,
                        Fqn = fieldSymbol.ToString(),
                        Types = fieldSymbol.Type.FindAllTypesRecursively(fieldSymbol.FindTypeArguemnts()),
                        Static = fieldSymbol.IsStatic,
                        Abstract = fieldSymbol.IsAbstract,
                        Sealed = fieldSymbol.IsSealed,
                        Required = fieldSymbol.IsRequired,
                        Override = fieldSymbol.IsOverride,
                        Virtual = fieldSymbol.IsVirtual,
                        Const = fieldSymbol.IsConst,
                        Volatile = fieldSymbol.IsVolatile
                    };

                    var literals = variableDeclaratorSyntax.DescendantNodes().OfType<LiteralExpressionSyntax>().ToList();
                    if (literals.Count != 0)
                    {
                        fieldModel.ConstantValue = literals[0].Token.IsKind(SyntaxKind.NullLiteralExpression) ? "null" : literals[0].Token.Value?.ToString();
                    }

                    fieldModel.Accessibility = fieldSymbol.DeclaredAccessibility.ToString();
                    memberOwningModel.Fields.Add(fieldModel);
                }
            }
        }
    }
}