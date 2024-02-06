﻿using System;
using System.Collections.Generic;
using System.Linq;
using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;
using IdentifierNameSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax;
using InvocationExpressionSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax;
using MemberAccessExpressionSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax;
using ObjectCreationExpressionSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.ObjectCreationExpressionSyntax;

namespace csharp_to_json_converter.utils.analyzers
{
    public class InvocationAnalyzer : AbstractAnalyzer
    {
        internal InvocationAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel) : base(syntaxTree,
            semanticModel)
        {
        }

        public void Analyze(SyntaxNode syntaxNode, MethodModel methodModel)
        {
            var invocationExpressions = syntaxNode.DescendantNodes().OfType<InvocationExpressionSyntax>().ToList();
            ProcessInvocations(invocationExpressions, methodModel);

            var objectCreationExpressions = syntaxNode.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().ToList();
            ProcessConstructors(objectCreationExpressions, methodModel);
            
            var identifierNames = syntaxNode.DescendantNodes().OfType<IdentifierNameSyntax>().ToList();
            ProcessPropertyAccesses(identifierNames, methodModel);
        }

        private void ProcessPropertyAccesses(List<IdentifierNameSyntax> identifierNames, MethodModel methodModel)
        {
            if (methodModel.Name == "InvokeAsync")
            {
                Console.WriteLine();
            }
            foreach (var nameSyntax in identifierNames)
            {
                var symbol = SemanticModel.GetSymbolInfo(nameSyntax).Symbol;
                if (symbol is null) { continue; }
                if (symbol.Kind != SymbolKind.Property) { continue; }

                var memberAccess = new MemberAccessModel
                {
                    LineNumber = nameSyntax.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    MemberId = symbol.ToString()
                };
                
                methodModel.MemberAccesses.Add(memberAccess);
            }
        }

        private void ProcessConstructors(List<ObjectCreationExpressionSyntax> objectCreations, MethodModel methodModel)
        {
            foreach (var objectCreation in objectCreations)
            {                
                var symbol = SemanticModel.GetSymbolInfo(objectCreation).Symbol;
                if (symbol is null) { return; }

                var invocationModel = new InvocationModel
                {
                    LineNumber = objectCreation.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    MethodId = symbol.ToString()
                };
                
                methodModel.Invocations.Add(invocationModel);
            }
        }
        
        private void ProcessInvocations(IEnumerable<InvocationExpressionSyntax> memberAccessExpressionSyntaxes,
            MethodModel methodModel)
        {
            foreach (var memberAccesses in memberAccessExpressionSyntaxes)
            {
                IMethodSymbol methodSymbol = null;

                if (memberAccesses.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax) {
                    methodSymbol = SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax).Symbol as IMethodSymbol;
                } 
                else if (memberAccesses.Expression is IdentifierNameSyntax identifierNameSyntax) {
                    methodSymbol = SemanticModel.GetSymbolInfo(identifierNameSyntax).Symbol as IMethodSymbol;
                }

                var invokesModel = new InvocationModel()
                {
                    LineNumber = memberAccesses.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    MethodId = methodSymbol?.ToString()
                };
                methodModel.Invocations.Add(invokesModel);
            }
        }
    }
}