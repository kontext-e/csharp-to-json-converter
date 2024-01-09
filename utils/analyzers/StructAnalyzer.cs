using System.IO;
using System.Linq;
using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace csharp_to_json_converter.utils.analyzers
{
    public class StructAnalyzer : AbstractAnalyzer
    {
        private readonly SemanticModel _semanticModel;
        private readonly SyntaxTree _syntaxTree;
        private readonly DirectoryInfo _inputDirectory;
        private readonly FieldAnalyzer _fieldAnalyzer;
        private readonly MethodAnalyzer _methodAnalyzer;
        private readonly ConstructorAnalyzer _constructorAnalyzer;
        private readonly PropertyAnalyzer _propertyAnalyzer;

        public StructAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel, DirectoryInfo inputDirectory) : base(syntaxTree, semanticModel)
        {
            _inputDirectory = inputDirectory;
            _syntaxTree = syntaxTree;
            _semanticModel = semanticModel;
            _fieldAnalyzer = new FieldAnalyzer(syntaxTree, semanticModel);
            _methodAnalyzer = new MethodAnalyzer(syntaxTree, semanticModel);
            _constructorAnalyzer = new ConstructorAnalyzer(syntaxTree, semanticModel);
            _propertyAnalyzer = new PropertyAnalyzer(syntaxTree, semanticModel);
        }

        public void Analyze(FileModel fileModel)
        {
            var structDeclarations = _syntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<StructDeclarationSyntax>()
                .ToList();

            foreach (var structDeclaration in structDeclarations)
            {
                var namedTypeSymbol = _semanticModel.GetDeclaredSymbol(structDeclaration) as INamedTypeSymbol;
                if (namedTypeSymbol == null) { continue; }
                
                var structModel = new StructModel
                {
                    Name = structDeclaration.Identifier.ValueText,
                    Fqn = _semanticModel.GetDeclaredSymbol(structDeclaration).ToString(),
                    Accessibility = namedTypeSymbol.DeclaredAccessibility.ToString(),
                    Sealed = namedTypeSymbol.IsSealed,
                    RelativePath = Path.GetRelativePath(_inputDirectory.FullName, fileModel.AbsolutePath),
                    Static = namedTypeSymbol.IsStatic,
                    Partial = structDeclaration.Modifiers.Any(modifier => modifier.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword)),
                    Md5 = BuildMD5(structDeclaration.GetText().ToString()),
                    FirstLineNumber = structDeclaration.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    LastLineNumber = structDeclaration.GetLocation().GetLineSpan().EndLinePosition.Line + 1
                };
                
                _fieldAnalyzer.Analyze(structDeclaration, structModel);
                _methodAnalyzer.Analyze(structDeclaration, structModel);
                _constructorAnalyzer.Analyze(structDeclaration, structModel);
                _propertyAnalyzer.Analyze(structDeclaration, structModel);
                
                fileModel.Structs.Add(structModel);
                
            }   

        }
    }
}