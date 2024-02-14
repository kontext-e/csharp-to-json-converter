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
        
        public void Analyze(TypeDeclarationSyntax typeDeclarationSyntax, MemberOwningModel memberOwningModel)
        {
            ConstructorModel primaryConstructor = AnalyzeForPrimaryConstructor(typeDeclarationSyntax);
            memberOwningModel.Constructors.Add(primaryConstructor);
            List<ConstructorModel> constructorModels = FindConstructors(typeDeclarationSyntax);
            memberOwningModel.Constructors.AddRange(constructorModels);
        }

        private ConstructorModel AnalyzeForPrimaryConstructor(TypeDeclarationSyntax typeDeclarationSyntax)
        {
            var parameterSyntaxes = typeDeclarationSyntax.ChildNodes().OfType<ParameterListSyntax>().ToList();
            if (parameterSyntaxes.Count == 0) { return null; }
            if (SemanticModel.GetDeclaredSymbol(typeDeclarationSyntax) is not INamedTypeSymbol typeSymbol) { return null; }
            
            var primaryConstructor = typeSymbol.InstanceConstructors[0];
            var constructorModel = new ConstructorModel
            {
                Fqn = primaryConstructor.ToString(),
                Name = primaryConstructor.ToString()![(primaryConstructor.ToString().LastIndexOf(".") + 1)..],
                Parameters = _parameterAnalyzer.CreateParameterModels(parameterSyntaxes[0].Parameters.ToList()),
                ReturnType = typeSymbol.ToString(),
                Accessibility = "Public",
                IsPrimaryConstructor = true,
                FirstLineNumber = typeDeclarationSyntax.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                LastLineNumber = typeDeclarationSyntax.GetLocation().GetLineSpan().StartLinePosition.Line + 1
            };

            return constructorModel;
        }

        private List<ConstructorModel> FindConstructors(SyntaxNode syntaxNode)
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
                    ReturnType = methodSymbol.ContainingType.ToString(),
                    Static = methodSymbol.IsStatic,
                    Abstract = methodSymbol.IsAbstract,
                    Sealed = methodSymbol.IsSealed,
                    Async = methodSymbol.IsAsync,
                    Override = methodSymbol.IsOverride,
                    Virtual = methodSymbol.IsVirtual,
                    IsPrimaryConstructor = false,
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