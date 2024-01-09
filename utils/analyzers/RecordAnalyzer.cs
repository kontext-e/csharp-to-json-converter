using System.IO;
using System.Linq;
using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace csharp_to_json_converter.utils.analyzers
{
    public class RecordAnalyzer : AbstractAnalyzer
    {
        private readonly SemanticModel _semanticModel;
        private readonly SyntaxTree _syntaxTree;
        private readonly DirectoryInfo _inputDirectory;
        private readonly FieldAnalyzer _fieldAnalyzer;
        private readonly MethodAnalyzer _methodAnalyzer;
        private readonly ConstructorAnalyzer _constructorAnalyzer;
        private readonly PropertyAnalyzer _propertyAnalyzer;

        public RecordAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel, DirectoryInfo inputDirectory) : base(syntaxTree, semanticModel)
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
            var recordDeclarations = _syntaxTree.GetRoot();
            var descendantNodes = recordDeclarations.DescendantNodes();
            var recordDeclarationSyntaxes = descendantNodes.OfType<RecordDeclarationSyntax>().ToList();

            foreach (var recordDeclaration in recordDeclarationSyntaxes)
            {
                var namedTypeSymbol = _semanticModel.GetDeclaredSymbol(recordDeclaration) as INamedTypeSymbol;
                if (namedTypeSymbol == null) { continue; }
                
                var recordModel = new RecordModel
                {
                    Name = recordDeclaration.Identifier.ValueText,
                    Fqn = _semanticModel.GetDeclaredSymbol(recordDeclaration).ToString(),
                    Accessibility = namedTypeSymbol.DeclaredAccessibility.ToString(),
                    Sealed = namedTypeSymbol.IsSealed,
                    RelativePath = Path.GetRelativePath(_inputDirectory.FullName, fileModel.AbsolutePath),
                    Static = namedTypeSymbol.IsStatic,
                    Partial = recordDeclaration.Modifiers.Any(modifier => modifier.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword)),
                    Md5 = BuildMD5(recordDeclaration.GetText().ToString()),
                    FirstLineNumber = recordDeclaration.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    LastLineNumber = recordDeclaration.GetLocation().GetLineSpan().EndLinePosition.Line + 1
                };
                
                _fieldAnalyzer.Analyze(recordDeclaration, recordModel);
                _methodAnalyzer.Analyze(recordDeclaration, recordModel);
                _constructorAnalyzer.Analyze(recordDeclaration, recordModel);
                _propertyAnalyzer.Analyze(recordDeclaration, recordModel);
                
                fileModel.Records.Add(recordModel);
                
            }   
        }
    }
}