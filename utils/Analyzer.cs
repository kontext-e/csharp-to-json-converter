using System.Collections.Generic;
using System.IO;
using System.Linq;
using csharp_to_json_converter.model;
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
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

            Logger.Info("Finished creating compilation.");
        }

        internal RootModel Analyze()
        {
            CreateCompilation();
            
            Logger.Info("Analyzing scripts ...");
            foreach (FileInfo fileInfo in _fileInfos)
            {
                AnalyzeScript(fileInfo);
            }

            Logger.Info("Finished analyzing scripts.");

            return new RootModel
            {
                FileModels = _fileModels
            };
        }

        private void AnalyzeScript(FileInfo fileInfo)
        {
            Logger.Debug("Analyzing script '{0}' ...", fileInfo.FullName);

            string scriptText = TryToReadFile(fileInfo);

            if (scriptText.Length == 0)
            {
                return;
            }

            FileModel fileModel = new FileModel();

            fileModel.AbsolutePath = fileInfo.FullName;

            SyntaxTree syntaxTree = _syntaxTrees[fileInfo.FullName];
            SemanticModel semanticModel = _compilation.GetSemanticModel(syntaxTree);

            List<ClassDeclarationSyntax> classDeclarationSyntaxes = syntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .ToList();

            foreach (ClassDeclarationSyntax classDeclarationSyntax in classDeclarationSyntaxes)
            {
                ClassModel classModel = new ClassModel
                {
                    Name = classDeclarationSyntax.Identifier.ValueText,
                    Fqn = semanticModel.GetDeclaredSymbol(classDeclarationSyntax).ToString()
                };

                fileModel.Classes.Add(classModel);
            }


            _fileModels.Add(fileModel);
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