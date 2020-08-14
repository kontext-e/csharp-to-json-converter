using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using csharp_to_json_converter.model;
using csharp_to_json_converter.utils.analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NLog;

namespace csharp_to_json_converter.utils
{
    public class Analyzer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private List<FileModel> _fileModels;
        private List<FileInfo> _fileInfos;
        private Dictionary<string, SyntaxTree> _syntaxTrees;
        private Compilation _compilation;

        public Analyzer(List<FileInfo> fileInfos)
        {
            _fileModels = new List<FileModel>();
            _fileInfos = fileInfos;
            _syntaxTrees = new Dictionary<string, SyntaxTree>();
        }

        private void CreateCompilation()
        {
            Logger.Info("Creating compilation ...");

            foreach (FileInfo fileInfo in _fileInfos)
            {
                string scriptText = TryToReadFile(fileInfo);
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(scriptText);
                _syntaxTrees.Add(fileInfo.FullName, syntaxTree);
            }

            _compilation = CSharpCompilation
                .Create("all")
                .AddSyntaxTrees(_syntaxTrees.Values)
                .AddReferences(
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)
                );

            Logger.Info("Finished creating compilation.");
        }

        internal List<FileModel> Analyze()
        {
            CreateCompilation();

            Logger.Info("Analyzing scripts ...");
            foreach (FileInfo fileInfo in _fileInfos)
            {
                AnalyzeScript(fileInfo);
            }

            Logger.Info("Finished analyzing scripts.");

            return _fileModels;
        }

        private void AnalyzeScript(FileInfo fileInfo)
        {
            Logger.Debug("Analyzing script '{0}' ...", fileInfo.FullName);

            string scriptText = TryToReadFile(fileInfo);

            if (scriptText.Length == 0)
            {
                return;
            }
            
            SyntaxTree syntaxTree = _syntaxTrees[fileInfo.FullName];
            SemanticModel semanticModel = _compilation.GetSemanticModel(syntaxTree);
            
            FileAnalyzer fileAnalyzer = new FileAnalyzer(syntaxTree, semanticModel);
            _fileModels.Add(fileAnalyzer.Analyze(fileInfo));
        }
        

        private void ReadInvocations(MethodDeclarationSyntax methodDeclarationSyntax, MethodModel methodModel,
            SemanticModel semanticModel)
        {
            List<InvocationExpressionSyntax> invocationExpressionSyntaxes = methodDeclarationSyntax
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .ToList();

            foreach (InvocationExpressionSyntax invocationExpressionSyntax in invocationExpressionSyntaxes)
            {
                IMethodSymbol methodSymbol = null;

                MemberAccessExpressionSyntax memberAccessExpressionSyntax =
                    invocationExpressionSyntax.Expression as MemberAccessExpressionSyntax;

                IdentifierNameSyntax identifierNameSyntax =
                    invocationExpressionSyntax.Expression as IdentifierNameSyntax;

                if (memberAccessExpressionSyntax != null)
                {
                    methodSymbol = semanticModel.GetSymbolInfo(memberAccessExpressionSyntax).Symbol as IMethodSymbol;
                }
                else if (identifierNameSyntax != null)
                {
                    methodSymbol = semanticModel.GetSymbolInfo(identifierNameSyntax).Symbol as IMethodSymbol;
                }

                if (methodSymbol == null)
                {
                    Logger.Warn("Unknown invocation expression.");
                    return;
                }

                InvokesModel invokesModel = new InvokesModel();
                invokesModel.LineNumber =
                    invocationExpressionSyntax.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                invokesModel.MethodId = methodSymbol.ToString();
                methodModel.Invocations.Add(invokesModel);
            }
        }

        private void ReadConstructors(ClassDeclarationSyntax classDeclarationSyntax, ClassModel classModel,
            SemanticModel semanticModel)
        {
            List<ConstructorDeclarationSyntax> constructorDeclarationSyntaxes = classDeclarationSyntax
                .DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .ToList();

            foreach (ConstructorDeclarationSyntax constructorDeclarationSyntax in constructorDeclarationSyntaxes)
            {
                ConstructorModel constructorModel = new ConstructorModel();

                constructorModel.Name = constructorDeclarationSyntax.Identifier.Text;

                IMethodSymbol methodSymbol = semanticModel.GetDeclaredSymbol(constructorDeclarationSyntax);

                constructorModel.Static = methodSymbol.IsStatic;
                constructorModel.Abstract = methodSymbol.IsAbstract;
                constructorModel.Sealed = methodSymbol.IsSealed;
                constructorModel.Async = methodSymbol.IsAsync;
                constructorModel.Override = methodSymbol.IsOverride;
                constructorModel.Virtual = methodSymbol.IsVirtual;
                constructorModel.Accessibility = methodSymbol.DeclaredAccessibility.ToString();

                classModel.Constructors.Add(constructorModel);
            }
        }

        private static string TryToReadFile(FileInfo fileInfo)
        {
            try
            {
                using StreamReader sr = new StreamReader(fileInfo.FullName);
                return sr.ReadToEnd();
            }
            catch (IOException e)
            {
                Logger.Error("The file '{0}' could not be read:\n\n{1}", fileInfo.FullName, e.Message);
            }

            return "";
        }
    }
}