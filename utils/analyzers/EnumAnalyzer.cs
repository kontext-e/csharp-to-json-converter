using System.Collections.Generic;
using System.Linq;
using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NLog;

namespace csharp_to_json_converter.utils.analyzers
{
    public class EnumAnalyzer : AbstractAnalyzer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        internal EnumAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel) : base(syntaxTree, semanticModel)
        {
        }

        internal void Analyze(FileModel fileModel)
        {
            List<EnumDeclarationSyntax> enumDeclarationSyntaxes = SyntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<EnumDeclarationSyntax>()
                .ToList();

            foreach (EnumDeclarationSyntax enumDeclarationSyntax in enumDeclarationSyntaxes)
            {
                INamedTypeSymbol namedTypeSymbol =
                    SemanticModel.GetDeclaredSymbol(enumDeclarationSyntax) as INamedTypeSymbol;

                EnumModel enumModel = new EnumModel();

                if (namedTypeSymbol == null)
                {
                    Logger.Warn("Found an enum that cannot be parsed: {}.", enumDeclarationSyntax);
                    continue;
                }

                enumModel.Fqn = namedTypeSymbol.ToString();
                enumModel.Name = namedTypeSymbol.Name;
                enumModel.Md5 = BuildMD5(enumDeclarationSyntax.GetText().ToString());

                List<EnumMemberDeclarationSyntax> enumMemberDeclarationSyntaxes = enumDeclarationSyntax
                    .DescendantNodes()
                    .OfType<EnumMemberDeclarationSyntax>()
                    .ToList();

                foreach (EnumMemberDeclarationSyntax enumMemberDeclarationSyntax in enumMemberDeclarationSyntaxes)
                {
                    ISymbol symbol = SemanticModel.GetDeclaredSymbol(enumMemberDeclarationSyntax);
                    enumModel.Members.Add(
                        new EnumMemberModel
                        {
                            Fqn = symbol.ToString(),
                            Name = symbol.Name
                        });
                }

                fileModel.Enums.Add(enumModel);
            }
        }
    }
}