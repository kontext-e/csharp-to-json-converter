using System.IO;
using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;

namespace csharp_to_json_converter.utils.analyzers
{
    public class FileAnalyzer : AbstractAnalyzer
    {
        private readonly ClassAnalyzer _classAnalyzer;
        private readonly UsingsAnalyzer _usingsAnalyzer;
        private readonly DirectoryInfo _inputDirectory;

        public FileAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel, DirectoryInfo inputDirectory) : base(
            syntaxTree, semanticModel)
        {
            _inputDirectory = inputDirectory;
            _classAnalyzer = new ClassAnalyzer(SyntaxTree, SemanticModel);
            _usingsAnalyzer = new UsingsAnalyzer(SyntaxTree, SemanticModel);
        }

        internal FileModel Analyze(FileInfo fileInfo)
        {
            FileModel fileModel = new FileModel();
            fileModel.AbsolutePath = fileInfo.FullName;
            fileModel.RelativePath = Path.GetRelativePath(_inputDirectory.FullName, fileInfo.FullName);

            _classAnalyzer.Analyze(fileModel);
            _usingsAnalyzer.Analyze(fileModel);

            return fileModel;
        }
    }
}