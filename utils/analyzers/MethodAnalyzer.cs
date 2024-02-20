using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
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

        public void Analyze(TypeDeclarationSyntax typeDeclarationSyntax, MemberOwningModel memberOwningModel)
        {
            List<MethodDeclarationSyntax> methodDeclarationSyntaxes = typeDeclarationSyntax
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .ToList();

            foreach (MethodDeclarationSyntax methodDeclarationSyntax in methodDeclarationSyntaxes)
            {
                var methodModel = new MethodModel();
                
                var methodSymbol = ModelExtensions.GetDeclaredSymbol(SemanticModel, methodDeclarationSyntax) as IMethodSymbol;
                if (methodSymbol == null) { continue; }

                methodModel.Name = methodDeclarationSyntax.Identifier.Text;
                methodModel.Fqn = methodSymbol.ToString();
                methodModel.Static = methodSymbol.IsStatic;
                methodModel.Abstract = methodSymbol.IsAbstract;
                methodModel.Sealed = methodSymbol.IsSealed;
                methodModel.Async = methodSymbol.IsAsync;
                methodModel.Override = methodSymbol.IsOverride;
                methodModel.Virtual = methodSymbol.IsVirtual;
                methodModel.Extern = methodSymbol.IsExtern;
                methodModel.Accessibility = methodSymbol.DeclaredAccessibility.ToString();
                methodModel.ReturnType = methodSymbol.ReturnType.ToString();
                methodModel.FirstLineNumber = methodDeclarationSyntax.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                methodModel.LastLineNumber = methodDeclarationSyntax.GetLocation().GetLineSpan().EndLinePosition.Line + 1;
                methodModel.Partial = methodDeclarationSyntax.Modifiers.Any(modifier =>
                    modifier.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword));
                
                if (IsExtensionMethod(methodDeclarationSyntax)) 
                {
                    methodModel.IsExtensionMethod = true;
                    methodModel.ExtendsType = methodSymbol.Parameters[0].Type.ToString();
                }
                
                if (!methodSymbol.IsAbstract)
                {
                    var controlFlowGraph = ControlFlowGraph.Create(methodDeclarationSyntax, SemanticModel, CancellationToken.None);
                    
                    methodModel.IsImplementation = controlFlowGraph is not null;
                    methodModel.CyclomaticComplexity = CalculateCyclomaticComplexity(controlFlowGraph);
                }
                
                _invocationAnalyzer.Analyze(methodDeclarationSyntax, methodModel);
                _parameterAnalyzer.Analyze(methodDeclarationSyntax, methodModel);

                memberOwningModel.Methods.Add(methodModel);
            }
        }

        private static int CalculateCyclomaticComplexity(ControlFlowGraph controlFlowGraph)
        {
            if (controlFlowGraph == null) { return 0; } 
            
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
            return numberOfEdges - numberOfBlocks + 2;
        }

        private static bool IsExtensionMethod(MethodDeclarationSyntax methodDeclarationSyntax)
        {
            if (methodDeclarationSyntax.ParameterList.Parameters.Count == 0) { return false; }
            
            //this Keyword to signal extension Method must be on the first Parameter
            var firstParameter = methodDeclarationSyntax.ParameterList.Parameters[0];
            return firstParameter.Modifiers.ToString().Contains("this");
        }
    }
}