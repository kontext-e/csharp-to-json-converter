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

        private List<ConstructorModel> FindContructors(SyntaxNode syntaxNode)
        {
            List<ConstructorDeclarationSyntax> constructorDeclarationSyntaxes = syntaxNode
                .DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .ToList();

            List<ConstructorModel> result = new List<ConstructorModel>();

            foreach (ConstructorDeclarationSyntax constructorDeclarationSyntax in constructorDeclarationSyntaxes)
            {
                ConstructorModel constructorModel = new ConstructorModel();

                constructorModel.Name = constructorDeclarationSyntax.Identifier.Text;

                IMethodSymbol methodSymbol =
                    SemanticModel.GetDeclaredSymbol(constructorDeclarationSyntax) as IMethodSymbol;

                constructorModel.Fqn = methodSymbol.ToString();
                constructorModel.Static = methodSymbol.IsStatic;
                constructorModel.Abstract = methodSymbol.IsAbstract;
                constructorModel.Sealed = methodSymbol.IsSealed;
                constructorModel.Async = methodSymbol.IsAsync;
                constructorModel.Override = methodSymbol.IsOverride;
                constructorModel.Virtual = methodSymbol.IsVirtual;
                constructorModel.Accessibility = methodSymbol.DeclaredAccessibility.ToString();
                constructorModel.FirstLineNumber =
                    constructorDeclarationSyntax.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                constructorModel.LastLineNumber =
                    constructorDeclarationSyntax.GetLocation().GetLineSpan().EndLinePosition.Line + 1;

                _invocationAnalyzer.Analyze(constructorDeclarationSyntax, constructorModel);
                _parameterAnalyzer.Analyze(constructorDeclarationSyntax, constructorModel);

                result.Add(constructorModel);
            }

            return result;
        }

        public void Analyze(InterfaceDeclarationSyntax interfaceDeclarationSyntax, InterfaceModel interfaceModel)
        {
            List<ConstructorModel> constructorModels = FindContructors(interfaceDeclarationSyntax);
            interfaceModel.Constructors.AddRange(constructorModels);
        }
    }
}