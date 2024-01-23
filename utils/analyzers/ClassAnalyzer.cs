using System.Collections.Generic;
using System.IO;
using System.Linq;
using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace csharp_to_json_converter.utils.analyzers
{
    public class ClassAnalyzer : AbstractAnalyzer
    {
        private readonly MethodAnalyzer _methodAnalyzer;
        private readonly ConstructorAnalyzer _constructorAnalyzer;
        private readonly FieldAnalyzer _fieldAnalyzer;
        private readonly PropertyAnalyzer _propertyAnalyzer;
        private readonly DirectoryInfo _inputDirectory;

        internal ClassAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel, DirectoryInfo inputDirectory) : base(syntaxTree, semanticModel)
        {
            _inputDirectory = inputDirectory;
            _methodAnalyzer = new MethodAnalyzer(SyntaxTree, SemanticModel);
            _constructorAnalyzer = new ConstructorAnalyzer(SyntaxTree, SemanticModel);
            _fieldAnalyzer = new FieldAnalyzer(SyntaxTree, SemanticModel);
            _propertyAnalyzer = new PropertyAnalyzer(SyntaxTree, SemanticModel);
        }

        internal void Analyze(FileModel fileModel)
        {
            List<ClassDeclarationSyntax> classDeclarationSyntaxes = SyntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .ToList();

            foreach (ClassDeclarationSyntax classDeclarationSyntax in classDeclarationSyntaxes)
            {
                var classModel = new ClassModel();
                AnalyzeType(fileModel, classDeclarationSyntax, classModel);
                fileModel.Classes.Add(classModel);
            }

            List<RecordDeclarationSyntax> recordDeclarationSyntaxes = SyntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<RecordDeclarationSyntax>()
                .ToList();

            foreach (var recordDeclarationSyntax in recordDeclarationSyntaxes)
            {
                if (recordDeclarationSyntax.Kind() != SyntaxKind.RecordDeclaration) { continue; }
                var recordClassModel = new RecordClassModel();
                AnalyzeType(fileModel, recordDeclarationSyntax, recordClassModel);
                fileModel.RecordClasses.Add(recordClassModel);
            }
        }

        private void AnalyzeType(FileModel fileModel, TypeDeclarationSyntax classDeclarationSyntax, ClassModel classModel)
        {
            var namedTypeSymbol = ModelExtensions.GetDeclaredSymbol(SemanticModel, classDeclarationSyntax) as INamedTypeSymbol;
            if (namedTypeSymbol == null) { return; }

            FillModel(fileModel, classModel, classDeclarationSyntax, namedTypeSymbol);
            AnalyzeMembers(classDeclarationSyntax, classModel);
        }

        private void AnalyzeMembers(TypeDeclarationSyntax classDeclarationSyntax, ClassModel classModel)
        {
            _fieldAnalyzer.Analyze(classDeclarationSyntax, classModel);
            _methodAnalyzer.Analyze(classDeclarationSyntax, classModel);
            _constructorAnalyzer.Analyze(classDeclarationSyntax, classModel);
            _propertyAnalyzer.Analyze(classDeclarationSyntax, classModel);
        }

        private void FillModel(FileModel fileModel, ClassModel classModel, TypeDeclarationSyntax classDeclarationSyntax,
            INamedTypeSymbol namedTypeSymbol)
        {
            classModel.Name = classDeclarationSyntax.Identifier.ValueText;
            classModel.Fqn = ModelExtensions.GetDeclaredSymbol(SemanticModel, classDeclarationSyntax)?.ToString();
            classModel.BaseType = namedTypeSymbol.BaseType?.ToString();
            classModel.Accessibility = namedTypeSymbol.DeclaredAccessibility.ToString();
            classModel.Abstract = namedTypeSymbol.IsAbstract;
            classModel.Sealed = namedTypeSymbol.IsSealed;
            classModel.RelativePath = Path.GetRelativePath(_inputDirectory.FullName, fileModel.AbsolutePath);
            classModel.Static = namedTypeSymbol.IsStatic;
            classModel.Partial = classDeclarationSyntax.Modifiers.Any(modifier => modifier.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword));
            classModel.Md5 = BuildMD5(classDeclarationSyntax.GetText().ToString());
            classModel.FirstLineNumber = classDeclarationSyntax.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            classModel.LastLineNumber = classDeclarationSyntax.GetLocation().GetLineSpan().EndLinePosition.Line + 1;
            
            foreach (INamedTypeSymbol interfaceTypeSymbol in namedTypeSymbol.Interfaces)
            {
                classModel.ImplementedInterfaces.Add(interfaceTypeSymbol.ToString());
            }
        }
    }
}