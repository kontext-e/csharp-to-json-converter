using System.IO;
using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;

namespace csharp_to_json_converter.utils.analyzers
{
    public class FileAnalyzer : AbstractAnalyzer
    {
        private readonly ClassAnalyzer _classAnalyzer;
        private readonly UsingsAnalyzer _usingsAnalyzer;

        public FileAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel) : base(syntaxTree, semanticModel)
        {
            _classAnalyzer = new ClassAnalyzer(SyntaxTree, SemanticModel);
            _usingsAnalyzer = new UsingsAnalyzer(SyntaxTree, SemanticModel);
        }

        internal FileModel Analyze(FileInfo fileInfo)
        {
            FileModel fileModel = new FileModel();
            fileModel.AbsolutePath = fileInfo.FullName;

            _classAnalyzer.Analyze(fileModel);
            _usingsAnalyzer.Analyze(fileModel);
            
            return fileModel;
        }
    }
}