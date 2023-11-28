using System.Collections.Generic;
using System.IO;
using System.Linq;
using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace csharp_to_json_converter.utils.analyzers
{
    public class InterfaceAnalyzer : AbstractAnalyzer
    {
        private readonly ConstructorAnalyzer _constructorAnalyzer;
        private readonly DirectoryInfo _inputDirectory;

        internal InterfaceAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel, DirectoryInfo inputDirectory) : base(syntaxTree, semanticModel)
        {
            _constructorAnalyzer = new ConstructorAnalyzer(SyntaxTree, SemanticModel);
            _inputDirectory = inputDirectory;
        }

        internal void Analyze(FileModel fileModel)
        {
            List<InterfaceDeclarationSyntax> interfaceDeclarationSyntaxes = SyntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<InterfaceDeclarationSyntax>()
                .ToList();

            foreach (InterfaceDeclarationSyntax interfaceDeclarationSyntax in interfaceDeclarationSyntaxes)
            {
                InterfaceModel interfaceModel = new InterfaceModel
                {
                    Name = interfaceDeclarationSyntax.Identifier.ValueText,
                    Fqn = SemanticModel.GetDeclaredSymbol(interfaceDeclarationSyntax).ToString(),
                    Md5 = BuildMD5(interfaceDeclarationSyntax.GetText().ToString())
                };

                INamedTypeSymbol namedTypeSymbol =
                    SemanticModel.GetDeclaredSymbol(interfaceDeclarationSyntax) as INamedTypeSymbol;
                
                foreach (INamedTypeSymbol interfaceTypeSymbol in namedTypeSymbol.Interfaces)
                {
                    interfaceModel.ImplementedInterfaces.Add(interfaceTypeSymbol.ToString());
                }

                interfaceModel.Accessibility = namedTypeSymbol.DeclaredAccessibility.ToString();
                interfaceModel.relativePath = Path.GetRelativePath(_inputDirectory.FullName, fileModel.AbsolutePath);
                interfaceModel.FirstLineNumber = interfaceDeclarationSyntax.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                interfaceModel.LastLineNumber = interfaceDeclarationSyntax.GetLocation().GetLineSpan().EndLinePosition.Line + 1;

                _constructorAnalyzer.Analyze(interfaceDeclarationSyntax, interfaceModel);

                fileModel.Interfaces.Add(interfaceModel);
            }
        }
    }
}