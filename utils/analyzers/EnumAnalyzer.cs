using System.Collections.Generic;
using System.IO;
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
        private readonly DirectoryInfo _inputDirectory;

        internal EnumAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel, DirectoryInfo inputDirectory) : base(syntaxTree, semanticModel)
        {
            _inputDirectory = inputDirectory;
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
                enumModel.RelativePath = Path.GetRelativePath(_inputDirectory.FullName, fileModel.AbsolutePath);
                enumModel.Md5 = BuildMD5(enumDeclarationSyntax.GetText().ToString());
                enumModel.FirstLineNumber = enumDeclarationSyntax.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                enumModel.LastLineNumber = enumDeclarationSyntax.GetLocation().GetLineSpan().EndLinePosition.Line + 1;
                enumModel.Accessibility = namedTypeSymbol.DeclaredAccessibility.ToString();

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