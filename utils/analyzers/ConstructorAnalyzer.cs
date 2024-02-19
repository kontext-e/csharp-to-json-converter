using System;
using System.Collections.Generic;
using System.Linq;
using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace csharp_to_json_converter.utils.analyzers
{
    public class ConstructorAnalyzer : AbstractAnalyzer
    {
        private readonly InvocationAnalyzer _invocationAnalyzer;
        private readonly ParameterAnalyzer _parameterAnalyzer;

        internal ConstructorAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel) : base(syntaxTree, semanticModel)
        {
            _invocationAnalyzer = new InvocationAnalyzer(SyntaxTree, SemanticModel);
            _parameterAnalyzer = new ParameterAnalyzer(SyntaxTree, SemanticModel);
        }
        
        public void Analyze(TypeDeclarationSyntax typeDeclarationSyntax, MemberOwningModel memberOwningModel)
        {
            AnalyzeForPrimaryConstructor(typeDeclarationSyntax, memberOwningModel);
            AnalyzeExplicitConstructors(typeDeclarationSyntax, memberOwningModel);
            AnalyzeForDefaultConstructor(typeDeclarationSyntax, memberOwningModel);
        }

        private void AnalyzeForPrimaryConstructor(TypeDeclarationSyntax typeDeclarationSyntax, MemberOwningModel memberOwningModel)
        {
            var parameterSyntaxes = typeDeclarationSyntax.ChildNodes().OfType<ParameterListSyntax>().ToList();
            if (parameterSyntaxes.Count == 0) return;
            if (ModelExtensions.GetDeclaredSymbol(SemanticModel, typeDeclarationSyntax) is not INamedTypeSymbol typeSymbol) return; 
            
            AddPrimaryConstructorWithArguments(typeDeclarationSyntax, memberOwningModel, typeSymbol, parameterSyntaxes);
            
            if (typeDeclarationSyntax.Kind() != SyntaxKind.RecordStructDeclaration) return;
            AddPrimaryConstructorWithoutArguments(typeDeclarationSyntax, memberOwningModel, typeSymbol);
        }

        private void AddPrimaryConstructorWithoutArguments(TypeDeclarationSyntax typeDeclarationSyntax, MemberOwningModel memberOwningModel,
            INamedTypeSymbol typeSymbol)
        {
            var noArgsConstructor = typeSymbol.InstanceConstructors[1];
            var model = CreateConstructorModel(typeDeclarationSyntax, noArgsConstructor, typeSymbol);
            memberOwningModel.Constructors.Add(model);
        }

        private void AddPrimaryConstructorWithArguments(TypeDeclarationSyntax typeDeclarationSyntax, MemberOwningModel memberOwningModel, INamedTypeSymbol typeSymbol,
            List<ParameterListSyntax> parameterSyntaxes)
        {
            var primaryConstructor = typeSymbol.InstanceConstructors[0];
            var constructorModel = CreateConstructorModel(typeDeclarationSyntax, primaryConstructor, typeSymbol);
            constructorModel.Parameters = _parameterAnalyzer.CreateParameterModels(parameterSyntaxes[0].Parameters.ToList());
            memberOwningModel.Constructors.Add(constructorModel);
        }

        private ConstructorModel CreateConstructorModel(TypeDeclarationSyntax typeDeclarationSyntax, IMethodSymbol primaryConstructor, INamedTypeSymbol typeSymbol)
        {
            var constructorModel = new ConstructorModel
            {
                Fqn = primaryConstructor.ToString(),
                Name = primaryConstructor.ToString()![(primaryConstructor.ToString().LastIndexOf(".") + 1)..primaryConstructor.ToString().IndexOf("(")],
                ReturnType = typeSymbol.ToString(),
                Accessibility = "Public",
                IsPrimaryConstructor = true,
                FirstLineNumber = typeDeclarationSyntax.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                LastLineNumber = typeDeclarationSyntax.GetLocation().GetLineSpan().StartLinePosition.Line + 1
            };
            return constructorModel;
        }

        private void AnalyzeExplicitConstructors(SyntaxNode syntaxNode, MemberOwningModel memberOwningModel)
        {
            List<ConstructorDeclarationSyntax> constructorDeclarationSyntaxes = syntaxNode
                .DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .ToList();
            
            foreach (var constructorDeclarationSyntax in constructorDeclarationSyntaxes)
            {
                if (ModelExtensions.GetDeclaredSymbol(SemanticModel, constructorDeclarationSyntax) is not IMethodSymbol methodSymbol) continue;
                var constructorModel = CreateConstructorModel(constructorDeclarationSyntax, methodSymbol);

                if (constructorDeclarationSyntax.DescendantNodes().OfType<ConstructorInitializerSyntax>().Any())
                {
                    var invocationModel = new InvocationModel
                    {
                        LineNumber = constructorDeclarationSyntax.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                        MethodId = memberOwningModel.Constructors[0].Fqn
                    };
                    constructorModel.Invocations.Add(invocationModel);
                }
                
                _invocationAnalyzer.Analyze(constructorDeclarationSyntax, constructorModel);
                _parameterAnalyzer.Analyze(constructorDeclarationSyntax, constructorModel);

                memberOwningModel.Constructors.Add(constructorModel);
            }
        }

        private ConstructorModel CreateConstructorModel(ConstructorDeclarationSyntax constructorDeclarationSyntax, IMethodSymbol methodSymbol)
        {
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
            return constructorModel;
        }

        private void AnalyzeForDefaultConstructor(TypeDeclarationSyntax typeDeclarationSyntax, MemberOwningModel memberOwningModel)
        {
            if (typeDeclarationSyntax.DescendantNodes().OfType<ConstructorDeclarationSyntax>().Any()
                || typeDeclarationSyntax.DescendantNodes().OfType<ParameterListSyntax>().Any()
                || memberOwningModel.Static) return;
            
            AddImplicitDefaultConstructor(typeDeclarationSyntax, memberOwningModel);
        }

        private void AddImplicitDefaultConstructor(TypeDeclarationSyntax typeDeclarationSyntax, MemberOwningModel memberOwningModel)
        {
            if (ModelExtensions.GetDeclaredSymbol(SemanticModel, typeDeclarationSyntax) is not INamedTypeSymbol typeSymbol) return;
            var constructor = FindPublicDefaultConstructor(typeSymbol);
            memberOwningModel.Constructors.Add(new ConstructorModel
            {
                Fqn = constructor.ToString(),
                Name = constructor.ToString()![(constructor.ToString().LastIndexOf(".") + 1)..constructor.ToString().IndexOf("(")],
                ReturnType = typeSymbol.ToString(),
                Accessibility = "Public",
                IsPrimaryConstructor = false,
                Parameters = new List<ParameterModel>()
            });
        }

        private static IMethodSymbol FindPublicDefaultConstructor(INamedTypeSymbol typeSymbol)
        {
            var constructor = typeSymbol.Constructors.Length > 1 ? 
                typeSymbol.Constructors.ToList().Find(c => c.DeclaredAccessibility.ToString().Equals("Public")) 
                : typeSymbol.Constructors[0];

            return constructor;
        }
    }
}