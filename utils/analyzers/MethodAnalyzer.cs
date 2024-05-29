using System.Collections.Generic;
using System.Linq;
using System.Threading;
using csharp_to_json_converter.model;
using csharp_to_json_converter.utils.ExtensionMethods;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace csharp_to_json_converter.utils.analyzers
{
    public class MethodAnalyzer : AbstractAnalyzer
    {
        private readonly InvocationAnalyzer _invocationAnalyzer;
        private readonly ParameterAnalyzer _parameterAnalyzer;

        internal MethodAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel, Solution solution) : base(syntaxTree, semanticModel)
        {
            _invocationAnalyzer = new InvocationAnalyzer(SyntaxTree, SemanticModel, solution);
            _parameterAnalyzer = new ParameterAnalyzer(SyntaxTree, SemanticModel);
        }

        public void Analyze(TypeDeclarationSyntax typeDeclarationSyntax, INamedTypeSymbol namedTypeSymbol, MemberOwningModel memberOwningModel)
        {
            AnalyzeExplicitMethods(typeDeclarationSyntax, memberOwningModel);
            AnalyzePropertyGetterSetter(namedTypeSymbol, memberOwningModel);
        }

        private void AnalyzePropertyGetterSetter(INamedTypeSymbol namedTypeSymbol, MemberOwningModel memberOwningModel)
        {
            foreach (var member in namedTypeSymbol.GetMembers())
            {
                if (member is not IMethodSymbol accessor) { continue; }
                if (accessor.MethodKind is not (MethodKind.PropertyGet or MethodKind.PropertySet)) { continue; }
                
                var methodModel = CreateAndFillMethodModel(accessor);
                methodModel.AssociatedProperty = accessor.AssociatedSymbol?.ToString();
                _invocationAnalyzer.ProcessInvocations(accessor, methodModel);
                _parameterAnalyzer.Analyze(accessor, methodModel);
                
                memberOwningModel.Methods.Add(methodModel);
            }
        }

        private void AnalyzeExplicitMethods(TypeDeclarationSyntax typeDeclarationSyntax, MemberOwningModel memberOwningModel)
        {
            List<MethodDeclarationSyntax> methodDeclarationSyntaxes = typeDeclarationSyntax
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .ToList();

            foreach (MethodDeclarationSyntax methodDeclarationSyntax in methodDeclarationSyntaxes)
            {
                if (SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax) is not IMethodSymbol methodSymbol) { continue; }

                var methodModel = CreateAndFillMethodModel(methodSymbol, methodDeclarationSyntax);
                if (!methodSymbol.IsAbstract)
                {
                    CalculateCyclomaticComplexity(methodDeclarationSyntax, methodModel);
                }
                
                _invocationAnalyzer.ProcessInvocations(methodSymbol, methodModel);
                _invocationAnalyzer.ProcessArrayCreations(methodDeclarationSyntax, methodModel);
                _parameterAnalyzer.Analyze(methodSymbol, methodModel);

                memberOwningModel.Methods.Add(methodModel);
            }
        }
        
        private MethodModel CreateAndFillMethodModel(IMethodSymbol methodSymbol,
            MethodDeclarationSyntax methodDeclarationSyntax = null)
        {
            var methodModel = new MethodModel
            {
                Name = methodSymbol.Name,
                Fqn = methodSymbol.ToString(),
                Static = methodSymbol.IsStatic,
                Abstract = methodSymbol.IsAbstract,
                Sealed = methodSymbol.IsSealed,
                Async = methodSymbol.IsAsync,
                Override = methodSymbol.IsOverride,
                Virtual = methodSymbol.IsVirtual,
                Extern = methodSymbol.IsExtern,
                Accessibility = methodSymbol.DeclaredAccessibility.ToString(),
                ReturnTypes = FindAllTypesRecursively(methodSymbol),
                FirstLineNumber = methodDeclarationSyntax?.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                LastLineNumber = methodDeclarationSyntax?.GetLocation().GetLineSpan().EndLinePosition.Line + 1,
                Partial = methodSymbol.PartialDefinitionPart != null ||  methodSymbol.PartialImplementationPart != null
            };

            if (methodSymbol.IsExtensionMethod)
            {
                AnalyzeForExtensionMethod(methodSymbol, methodModel);
            }
            
            return methodModel;
        }

        private static void AnalyzeForExtensionMethod(IMethodSymbol methodSymbol, MethodModel methodModel)
        {
            methodModel.IsExtensionMethod = true;
            methodModel.ExtendsType = methodSymbol.Parameters[0].Type.ToString();
        }

        private void CalculateCyclomaticComplexity(MethodDeclarationSyntax methodDeclarationSyntax, MethodModel methodModel)
        {
            var controlFlowGraph = ControlFlowGraph.Create(methodDeclarationSyntax, SemanticModel, CancellationToken.None);
                    
            methodModel.IsImplementation = controlFlowGraph is not null;

            if (controlFlowGraph == null) { return; } 
            
            int numberOfBlocks = controlFlowGraph.Blocks.Length;
            int numberOfEdges = 0;
            foreach (BasicBlock basicBlock in controlFlowGraph.Blocks)
            {
                if (basicBlock.ConditionalSuccessor != null)
                {
                    numberOfEdges++;
                }

                if (basicBlock.FallThroughSuccessor != null)
                {
                    numberOfEdges++;
                }
            }
            methodModel.CyclomaticComplexity =  numberOfEdges - numberOfBlocks + 2;
        }

        private static IEnumerable<string> FindAllTypesRecursively(IMethodSymbol methodSymbol)
        {
            switch (methodSymbol.ReturnType)
            {
                case INamedTypeSymbol namedTypeSymbol:
                    return namedTypeSymbol.GetAllTypes(methodSymbol);
                case ITypeParameterSymbol typeParameterSymbol:
                    return typeParameterSymbol.GetAllTypes(methodSymbol);
                case IArrayTypeSymbol arrayTypeSymbol:
                    return arrayTypeSymbol.GetAllTypes(methodSymbol);
                default:
                    return new List<string>();
            }
        }

    }
}