using System.IO;
using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;

namespace csharp_to_json_converter.utils.analyzers
{
    public class FileAnalyzer : AbstractAnalyzer
    {
        private readonly EnumAnalyzer _enumAnalyzer;
        private readonly ClassAnalyzer _classAnalyzer;
        private readonly UsingsAnalyzer _usingsAnalyzer;
        private readonly InterfaceAnalyzer _interfaceAnalyzer;
        private readonly DirectoryInfo _inputDirectory;

        public FileAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel, DirectoryInfo inputDirectory) : base(
            syntaxTree, semanticModel)
        {
            _inputDirectory = inputDirectory;
            _enumAnalyzer = new EnumAnalyzer(SyntaxTree, SemanticModel);
            _classAnalyzer = new ClassAnalyzer(SyntaxTree, SemanticModel);
            _usingsAnalyzer = new UsingsAnalyzer(SyntaxTree, SemanticModel);
            _interfaceAnalyzer = new InterfaceAnalyzer(SyntaxTree, SemanticModel);
        }

        internal FileModel Analyze(FileInfo fileInfo)
        {
            FileModel fileModel = new FileModel();
            fileModel.Name = fileInfo.Name;
            fileModel.AbsolutePath = fileInfo.FullName;
            fileModel.RelativePath = Path.GetRelativePath(_inputDirectory.FullName, fileInfo.FullName);

            _interfaceAnalyzer.Analyze(fileModel);
            _enumAnalyzer.Analyze(fileModel);
            _classAnalyzer.Analyze(fileModel);
            _usingsAnalyzer.Analyze(fileModel);

            return fileModel;
        }
    }
}