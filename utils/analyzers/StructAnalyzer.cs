using System.IO;
using System.Linq;
using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
                var structModel = new StructModel();
                AnalyzeType(fileModel, structDeclaration, structModel);
                fileModel.Structs.Add(structModel);
            }

            var recordStructDeclarations = _syntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<RecordDeclarationSyntax>()
                .ToList();
            
            foreach (var recordStructDeclaration in recordStructDeclarations)
            {
                if (recordStructDeclaration.Kind() != SyntaxKind.RecordStructDeclaration) { continue; }
                var recordStructModel = new RecordStructModel();
                AnalyzeType(fileModel, recordStructDeclaration, recordStructModel);
                fileModel.RecordStructs.Add(recordStructModel);
            }
        }

        private void AnalyzeType(FileModel fileModel, TypeDeclarationSyntax structDeclaration, StructModel structModel)
        {
            var namedTypeSymbol = ModelExtensions.GetDeclaredSymbol(_semanticModel, structDeclaration) as INamedTypeSymbol;
            if (namedTypeSymbol == null) { return; }

            FillModel(fileModel, structModel, structDeclaration, namedTypeSymbol);
            AnalyzeMembers(structDeclaration, structModel);
        }

        private void AnalyzeMembers(TypeDeclarationSyntax structDeclaration, StructModel structModel)
        {
            _fieldAnalyzer.Analyze(structDeclaration, structModel);
            _methodAnalyzer.Analyze(structDeclaration, structModel);
            _constructorAnalyzer.Analyze(structDeclaration, structModel);
            _propertyAnalyzer.Analyze(structDeclaration, structModel);
        }

        private void FillModel(FileModel fileModel, StructModel structModel, TypeDeclarationSyntax structDeclaration,
            INamedTypeSymbol namedTypeSymbol)
        {
            structModel.Name = structDeclaration.Identifier.ValueText;
            structModel.Fqn = ModelExtensions.GetDeclaredSymbol(_semanticModel, structDeclaration)?.ToString();
            structModel.Accessibility = namedTypeSymbol.DeclaredAccessibility.ToString();
            structModel.Sealed = namedTypeSymbol.IsSealed;
            structModel.RelativePath = Path.GetRelativePath(_inputDirectory.FullName, fileModel.AbsolutePath);
            structModel.Static = namedTypeSymbol.IsStatic;
            structModel.Partial = structDeclaration.Modifiers.Any(modifier => modifier.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword));
            structModel.Md5 = BuildMD5(structDeclaration.GetText().ToString());
            structModel.FirstLineNumber = structDeclaration.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            structModel.LastLineNumber = structDeclaration.GetLocation().GetLineSpan().EndLinePosition.Line + 1;
        }
    }
}