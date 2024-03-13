using System.Linq;
using csharp_to_json_converter.model;
using csharp_to_json_converter.utils.ExtensionMethods;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace csharp_to_json_converter.utils.analyzers
{
    public class ConstructorAnalyzer : AbstractAnalyzer
    {
        private readonly InvocationAnalyzer _invocationAnalyzer;
        private readonly ParameterAnalyzer _parameterAnalyzer;

        internal ConstructorAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel, Solution solution) : base(syntaxTree, semanticModel)
        {
            _invocationAnalyzer = new InvocationAnalyzer(SyntaxTree, SemanticModel, solution);
            _parameterAnalyzer = new ParameterAnalyzer(SyntaxTree, SemanticModel);
        }
        
        public void Analyze(TypeDeclarationSyntax typeDeclarationSyntax, MemberOwningModel memberOwningModel)
        {
            var namedTypeSymbol = SemanticModel.GetDeclaredSymbol(typeDeclarationSyntax);
            if (namedTypeSymbol == null) return;

            foreach (var constructor in namedTypeSymbol.InstanceConstructors)
            {
                AnalyzeConstructors(constructor, typeDeclarationSyntax, memberOwningModel);                
            }
        }

        private void AnalyzeConstructors(IMethodSymbol methodSymbol, TypeDeclarationSyntax typeDeclarationSyntax, MemberOwningModel memberOwningModel)
        {
                var constructorModel = CreateConstructorModel(methodSymbol);
                _invocationAnalyzer.ProcessInvocations(methodSymbol, constructorModel);
                _parameterAnalyzer.Analyze(methodSymbol, constructorModel);

                memberOwningModel.Constructors.Add(constructorModel);
        }

        private ConstructorModel CreateConstructorModel(IMethodSymbol methodSymbol)
        {
            var locations = methodSymbol.Locations.ToList();
            var location = locations.Count == 1 ? locations[0] : null;
            var constructorModel = new ConstructorModel
            {
                Name = methodSymbol.MetadataName,
                Fqn = methodSymbol.getFqn(),
                ReturnType = methodSymbol.ContainingType.ToString(),
                Static = methodSymbol.IsStatic,
                Abstract = methodSymbol.IsAbstract,
                Sealed = methodSymbol.IsSealed,
                Async = methodSymbol.IsAsync,
                Override = methodSymbol.IsOverride,
                Virtual = methodSymbol.IsVirtual,
                IsPrimaryConstructor = false,
                Accessibility = methodSymbol.DeclaredAccessibility.ToString(),
                FirstLineNumber = location?.GetLineSpan().StartLinePosition.Line,
                LastLineNumber = location?.GetLineSpan().EndLinePosition.Line
            };
            return constructorModel;
        }
    }
}