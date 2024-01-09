using System.Collections.Generic;
using System.IO;
using System.Linq;
using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;
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
                INamedTypeSymbol namedTypeSymbol =
                    SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) as INamedTypeSymbol;
                if (namedTypeSymbol == null) { continue; }

                var classModel = new ClassModel
                {
                    Name = classDeclarationSyntax.Identifier.ValueText,
                    Fqn = SemanticModel.GetDeclaredSymbol(classDeclarationSyntax).ToString(),
                    BaseType = namedTypeSymbol.BaseType?.ToString(),
                    Accessibility = namedTypeSymbol.DeclaredAccessibility.ToString(),
                    Abstract = namedTypeSymbol.IsAbstract,
                    Sealed = namedTypeSymbol.IsSealed,
                    RelativePath = Path.GetRelativePath(_inputDirectory.FullName, fileModel.AbsolutePath),
                    Static = namedTypeSymbol.IsStatic,
                    Partial = classDeclarationSyntax.Modifiers.Any(modifier => modifier.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword)),
                    Md5 = BuildMD5(classDeclarationSyntax.GetText().ToString()),
                    FirstLineNumber = classDeclarationSyntax.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    LastLineNumber = classDeclarationSyntax.GetLocation().GetLineSpan().EndLinePosition.Line + 1
                };


                foreach (INamedTypeSymbol interfaceTypeSymbol in namedTypeSymbol.Interfaces)
                {
                    classModel.ImplementedInterfaces.Add(interfaceTypeSymbol.ToString());
                }

                _fieldAnalyzer.Analyze(classDeclarationSyntax, classModel);
                _methodAnalyzer.Analyze(classDeclarationSyntax, classModel);
                _constructorAnalyzer.Analyze(classDeclarationSyntax, classModel);
                _propertyAnalyzer.Analyze(classDeclarationSyntax, classModel);

                fileModel.Classes.Add(classModel);
            }
        }
    }
}