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

        public void Analyze(ClassDeclarationSyntax classDeclarationSyntax, ClassModel classModel)
        {
            List<FieldDeclarationSyntax> fieldDeclarationSyntaxes = classDeclarationSyntax
                .DescendantNodes()
                .OfType<FieldDeclarationSyntax>()
                .ToList();

            foreach (FieldDeclarationSyntax fieldDeclarationSyntax in fieldDeclarationSyntaxes)
            {
                foreach (VariableDeclaratorSyntax variableDeclaratorSyntax in fieldDeclarationSyntax.Declaration
                    .Variables)
                {
                    IFieldSymbol fieldSymbol =
                        SemanticModel.GetDeclaredSymbol(variableDeclaratorSyntax) as IFieldSymbol;

                    FieldModel fieldModel = new FieldModel();
                    fieldModel.Name = fieldSymbol.Name;
                    fieldModel.Fqn = fieldSymbol.ToString();
                    fieldModel.Type = fieldSymbol.Type.ToString();
                    fieldModel.Static = fieldSymbol.IsStatic;
                    fieldModel.Abstract = fieldSymbol.IsAbstract;
                    fieldModel.Sealed = fieldSymbol.IsSealed;
                    fieldModel.Override = fieldSymbol.IsOverride;
                    fieldModel.Virtual = fieldSymbol.IsVirtual;
                    fieldModel.Const = fieldSymbol.IsConst;
                    fieldModel.Volatile = fieldSymbol.IsVolatile;

                    if (fieldSymbol.HasConstantValue)
                    {
                        fieldModel.ConstantValue = fieldSymbol.ConstantValue.ToString();
                    }

                    fieldModel.Accessibility = fieldSymbol.DeclaredAccessibility.ToString();
                    classModel.Fields.Add(fieldModel);
                }
            }
        }
    }
}