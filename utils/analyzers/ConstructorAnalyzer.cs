using System.Collections.Generic;
using System.Linq;
using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace csharp_to_json_converter.utils.analyzers
{
    public class ConstructorAnalyzer : AbstractAnalyzer
    {
        private readonly InvocationAnalyzer _invocationAnalyzer;
        private readonly ParameterAnalyzer _parameterAnalyzer;

        internal ConstructorAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel) : base(syntaxTree,
            semanticModel)
        {
            _invocationAnalyzer = new InvocationAnalyzer(SyntaxTree, SemanticModel);
            _parameterAnalyzer = new ParameterAnalyzer(SyntaxTree, SemanticModel);
        }

        public void Analyze(ClassDeclarationSyntax classDeclarationSyntax, ClassModel classModel)
        {
            List<ConstructorModel> constructorModels = FindContructors(classDeclarationSyntax);
            classModel.Constructors.AddRange(constructorModels);
        }

        public void Analyze(TypeDeclarationSyntax interfaceDeclarationSyntax, MemberOwningModel interfaceModel)
        {
            List<ConstructorModel> constructorModels = FindContructors(interfaceDeclarationSyntax);
            interfaceModel.Constructors.AddRange(constructorModels);
        }

        private List<ConstructorModel> FindContructors(SyntaxNode syntaxNode)
        {
            List<ConstructorDeclarationSyntax> constructorDeclarationSyntaxes = syntaxNode
                .DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .ToList();

            List<ConstructorModel> result = new List<ConstructorModel>();

            foreach (ConstructorDeclarationSyntax constructorDeclarationSyntax in constructorDeclarationSyntaxes)
            {
                IMethodSymbol methodSymbol = SemanticModel.GetDeclaredSymbol(constructorDeclarationSyntax) as IMethodSymbol;
                if (methodSymbol == null) { continue; }
                
                ConstructorModel constructorModel = new ConstructorModel
                {
                    Name = constructorDeclarationSyntax.Identifier.Text,
                    Fqn = methodSymbol.ToString(),
                    Static = methodSymbol.IsStatic,
                    Abstract = methodSymbol.IsAbstract,
                    Sealed = methodSymbol.IsSealed,
                    Async = methodSymbol.IsAsync,
                    Override = methodSymbol.IsOverride,
                    Virtual = methodSymbol.IsVirtual,
                    Accessibility = methodSymbol.DeclaredAccessibility.ToString(),
                    FirstLineNumber = constructorDeclarationSyntax.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    LastLineNumber = constructorDeclarationSyntax.GetLocation().GetLineSpan().EndLinePosition.Line + 1
                };

                _invocationAnalyzer.Analyze(constructorDeclarationSyntax, constructorModel);
                _parameterAnalyzer.Analyze(constructorDeclarationSyntax, constructorModel);

                result.Add(constructorModel);
            }

            return result;
        }
    }
}