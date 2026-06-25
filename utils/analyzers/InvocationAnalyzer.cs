using System.Collections.Generic;
using System.Linq;
using csharp_to_json_converter.model;
using csharp_to_json_converter.utils.ExtensionMethods;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace csharp_to_json_converter.utils.analyzers
{
    public class InvocationAnalyzer : AbstractAnalyzer
    {
        internal InvocationAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel) : base(syntaxTree, semanticModel) { }

        public void ProcessInvocations(MethodDeclarationSyntax methodDeclarationSyntax,
            MethodModel methodModel,
            IMethodSymbol callerSymbol)
        {
            var invocations = methodDeclarationSyntax
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>();

            ProcessInvocationNodes(invocations, methodModel, callerSymbol, SemanticModel);
            ProcessObjectCreationNodes(methodDeclarationSyntax, methodModel, callerSymbol, SemanticModel);
        }

        public void ProcessInvocations(IMethodSymbol callerSymbol, MethodModel methodModel)
        {
            var syntaxReference = callerSymbol.DeclaringSyntaxReferences.FirstOrDefault();
            if (syntaxReference == null) return;

            var accessorSyntax = syntaxReference.GetSyntax();
            var correctSemanticModel = Analyzer.FindSemanticModelForFileContainingSyntaxNode(accessorSyntax);
            if (correctSemanticModel == null) return;

            var invocations = accessorSyntax
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>();

            ProcessInvocationNodes(invocations, methodModel, callerSymbol, correctSemanticModel);
            ProcessObjectCreationNodes(accessorSyntax, methodModel, callerSymbol, correctSemanticModel);
        }

        private void ProcessInvocationNodes(IEnumerable<InvocationExpressionSyntax> invocations,
            MethodModel methodModel,
            IMethodSymbol callerSymbol,
            SemanticModel semanticModel)
        {
            foreach (var invocation in invocations)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(invocation);
                var calleeSymbol = (symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault())
                    as IMethodSymbol;

                if (calleeSymbol == null) continue;

                methodModel.Invokes.Add(CreateInvokeModel(calleeSymbol, invocation, callerSymbol));
            }
        }

        private InvocationModel CreateInvokeModel(IMethodSymbol calleeSymbol, 
            ExpressionSyntax invocationSyntax,
            IMethodSymbol callerSymbol)
        {
            return new InvocationModel
            {
                MethodId = calleeSymbol.ToString(),
                LineNumber = invocationSyntax.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                TypeArguments = AnalyzeTypeArguments(calleeSymbol, callerSymbol)
            };
        }

        private List<string> AnalyzeTypeArguments(IMethodSymbol calleeSymbol, IMethodSymbol callerSymbol)
        {
            var allTypesPerTypeArgument = calleeSymbol.TypeArguments.Select(typeArg =>
                typeArg.FindAllTypes(callerSymbol.FindTypeArguments()));

            return allTypesPerTypeArgument.SelectMany(x => x).ToList();
        }
        
        private void ProcessObjectCreationNodes(SyntaxNode scopeSyntax,
            MethodModel methodModel,
            IMethodSymbol callerSymbol,
            SemanticModel semanticModel)
        {
            // Handles: new Foo(), new Foo(args), new Foo { ... }
            var explicitCreations = scopeSyntax
                .DescendantNodes()
                .OfType<ObjectCreationExpressionSyntax>();

            foreach (var creation in explicitCreations)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(creation);
                var calleeSymbol = (symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault())
                    as IMethodSymbol;
                if (calleeSymbol == null) continue;
                methodModel.Invokes.Add(CreateInvokeModel(calleeSymbol, creation, callerSymbol));
            }

            // Handles: new() — implicit object creation (target-typed new)
            var implicitCreations = scopeSyntax
                .DescendantNodes()
                .OfType<ImplicitObjectCreationExpressionSyntax>();

            foreach (var creation in implicitCreations)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(creation);
                var calleeSymbol = (symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault())
                    as IMethodSymbol;
                if (calleeSymbol == null) continue;
                methodModel.Invokes.Add(CreateInvokeModel(calleeSymbol, creation, callerSymbol));
            }
        }

        public void ProcessArrayCreations(MethodDeclarationSyntax methodDeclarationSyntax, MethodModel methodModel)
        {
            var arrayCreations = methodDeclarationSyntax.DescendantNodes().OfType<VariableDeclarationSyntax>().ToList();

            foreach (var arrayCreation in arrayCreations)
            {
                var arrayTypeSyntax = arrayCreation.ChildNodes().ToList()[0];
                if (SemanticModel.GetSymbolInfo(arrayTypeSyntax).Symbol is not IArrayTypeSymbol arrayTypeSymbol) continue;

                methodModel.CreatesArrays.Add( new ArrayCreationModel 
                {
                    Type = arrayTypeSymbol.ToString(),
                    LineNumber = arrayCreation.GetLocation().GetLineSpan().StartLinePosition.Line + 1
                });
            }
        }

    }
}