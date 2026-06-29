using System.Collections.Generic;
using System.Linq;
using csharp_to_json_converter.model;
using csharp_to_json_converter.utils.ExtensionMethods;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
            ProcessPropertyAccessors(methodDeclarationSyntax, methodModel, callerSymbol, SemanticModel);
        }

        public void ProcessInvocations(IMethodSymbol callerSymbol, MethodModel methodModel)
        {
            var syntaxReference = callerSymbol.DeclaringSyntaxReferences.FirstOrDefault();
            if (syntaxReference == null) return;

            var scopeSyntax = syntaxReference.GetSyntax();
            var correctSemanticModel = Analyzer.FindSemanticModelForFileContainingSyntaxNode(scopeSyntax);
            if (correctSemanticModel == null) return;

            var invocations = scopeSyntax
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>();

            ProcessInvocationNodes(invocations, methodModel, callerSymbol, correctSemanticModel);
            ProcessObjectCreationNodes(scopeSyntax, methodModel, callerSymbol, correctSemanticModel);
            ProcessConstructorInitializer(scopeSyntax, methodModel, callerSymbol, correctSemanticModel);
            ProcessPropertyAccessors(scopeSyntax, methodModel, callerSymbol, correctSemanticModel);
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
            SyntaxNode syntaxNode,
            IMethodSymbol callerSymbol)
        {
            // Keep substituted symbol for type argument analysis
            var substitutedSymbol = calleeSymbol;

            // Normalize MethodId to match declaration FQN:
            // - Extension methods: keep reduced form (TypeClass.Method(double))
            //   since that's what the declaration side also stores via GetFqn()
            // - Generic methods: strip type substitution to get back to <T> form
            var methodIdSymbol = calleeSymbol;
            if (methodIdSymbol.IsGenericMethod && !methodIdSymbol.TypeArguments.IsEmpty)
                methodIdSymbol = methodIdSymbol.OriginalDefinition;

            return new InvocationModel
            {
                MethodId = methodIdSymbol.ToString(),
                LineNumber = syntaxNode.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                TypeArguments = AnalyzeTypeArguments(substitutedSymbol, callerSymbol)
            };
        }
        // Existing one now delegates to the above
        private InvocationModel CreateInvokeModel(IMethodSymbol calleeSymbol,
            ExpressionSyntax expressionSyntax,
            IMethodSymbol callerSymbol)
            => CreateInvokeModel(calleeSymbol, (SyntaxNode)expressionSyntax, callerSymbol);

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
                var symbolInfo = ModelExtensions.GetSymbolInfo(semanticModel, creation);
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
                var symbolInfo = ModelExtensions.GetSymbolInfo(semanticModel, creation);
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

        private void ProcessPropertyAccessors(SyntaxNode scopeSyntax,
            MethodModel methodModel,
            IMethodSymbol callerSymbol,
            SemanticModel semanticModel)
        {
            var propertyAccesses = scopeSyntax
                .DescendantNodes()
                .Where(node => node is MemberAccessExpressionSyntax or IdentifierNameSyntax);

            foreach (var node in propertyAccesses)
            {
                var symbol = semanticModel.GetSymbolInfo(node).Symbol;
                if (symbol is not IPropertySymbol propertySymbol) continue;

                // Compound assignment (+=, -=, *=, etc.) invokes both getter and setter
                var isCompoundWrite = node.Parent is AssignmentExpressionSyntax compoundAssignment
                                      && compoundAssignment.Left == node
                                      && !compoundAssignment.IsKind(SyntaxKind.SimpleAssignmentExpression);

                if (isCompoundWrite)
                {
                    if (propertySymbol.GetMethod != null)
                        methodModel.Invokes.Add(CreateInvokeModel(propertySymbol.GetMethod, node, callerSymbol));
                    if (propertySymbol.SetMethod != null)
                        methodModel.Invokes.Add(CreateInvokeModel(propertySymbol.SetMethod, node, callerSymbol));
                    continue;
                }

                // Simple assignment (=) invokes only the setter
                // Everything else (reads) invokes only the getter
                var isSimpleWrite = node.Parent is AssignmentExpressionSyntax simpleAssignment
                                    && simpleAssignment.Left == node
                                    && simpleAssignment.IsKind(SyntaxKind.SimpleAssignmentExpression);

                var accessorSymbol = isSimpleWrite
                    ? propertySymbol.SetMethod
                    : propertySymbol.GetMethod;

                if (accessorSymbol == null) continue;

                methodModel.Invokes.Add(CreateInvokeModel(accessorSymbol, node, callerSymbol));
            }
        }        
        
        private void ProcessConstructorInitializer(SyntaxNode scopeSyntax,
            MethodModel methodModel,
            IMethodSymbol callerSymbol,
            SemanticModel semanticModel)
        {
            var initializer = scopeSyntax
                .DescendantNodes()
                .OfType<ConstructorInitializerSyntax>()
                .FirstOrDefault();

            if (initializer == null) return;

            var symbolInfo = ModelExtensions.GetSymbolInfo(semanticModel, initializer);
            var calleeSymbol = (symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault())
                as IMethodSymbol;

            if (calleeSymbol == null) return;

            methodModel.Invokes.Add(CreateInvokeModel(calleeSymbol, initializer, callerSymbol));
        }
    }
}