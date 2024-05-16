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
        private readonly StructAnalyzer _structAnalyzer;

        public FileAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel, DirectoryInfo inputDirectory, Solution solution) : base(
            syntaxTree, semanticModel)
        {
            _inputDirectory = inputDirectory;
            _enumAnalyzer = new EnumAnalyzer(SyntaxTree, SemanticModel, _inputDirectory);
            _classAnalyzer = new ClassAnalyzer(SyntaxTree, SemanticModel, _inputDirectory, solution);
            _usingsAnalyzer = new UsingsAnalyzer(SyntaxTree, SemanticModel);
            _interfaceAnalyzer = new InterfaceAnalyzer(SyntaxTree, SemanticModel, _inputDirectory, solution);
            _structAnalyzer = new StructAnalyzer(SyntaxTree, SemanticModel, _inputDirectory, solution);
        }

        internal FileModel Analyze(Document fileInfo)
        {
            var fileModel = new FileModel
            {
                Name = fileInfo.Name,
                AbsolutePath = fileInfo.FilePath,
                RelativePath = Path.GetRelativePath(_inputDirectory.FullName, fileInfo.FilePath)
            };

            _interfaceAnalyzer.Analyze(fileModel);
            _enumAnalyzer.Analyze(fileModel);
            _classAnalyzer.Analyze(fileModel);
            _structAnalyzer.Analyze(fileModel);
            _usingsAnalyzer.Analyze(fileModel);

            return fileModel;
        }
    }
}