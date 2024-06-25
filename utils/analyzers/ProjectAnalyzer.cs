using System.IO;
using System.Linq;
using csharp_to_json_converter.model;
using csharp_to_json_converter.utils.ExtensionMethods;
using Microsoft.CodeAnalysis;
using NLog;

namespace csharp_to_json_converter.utils.analyzers;

public class ProjectAnalyzer
{
    private readonly DirectoryInfo _inputDirectory;
    private readonly Solution _solution;
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public ProjectAnalyzer(DirectoryInfo inputDirectory, Solution solution)
    {
        _inputDirectory = inputDirectory;
        _solution = solution;
    }

    internal ProjectModel Analyze(Project project)
    {
        ProjectModel projectModel = new();
        projectModel.Name = project.Name;
        projectModel.AbsolutePath = project.FilePath;
        projectModel.RelativePath = Path.GetRelativePath(_solution.FilePath!, project.FilePath!);
        
        foreach (var fileInfo in project.Documents)
        {
            if (fileInfo.IsEmpty() || fileInfo.IsExcluded()) continue;
            Logger.Debug("Analyzing script '{0}' ...", fileInfo.FilePath);
            projectModel.FileModels.Add(AnalyzeScript(fileInfo, Analyzer.Compilations[project.Name]));
        }

        return projectModel;
    }

    private FileModel AnalyzeScript(Document fileInfo, Compilation compilation)
    {
        var syntaxTree = compilation.SyntaxTrees.ToList().Find(syntaxTree => syntaxTree.FilePath.Equals(fileInfo.FilePath))!;
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        
        var fileAnalyzer = new FileAnalyzer(syntaxTree, semanticModel, _inputDirectory, _solution);
        var fileModel = fileAnalyzer.Analyze(fileInfo);
        Logger.Info("Analyzed File " + ++Analyzer.ScannedFiles + "/" + Analyzer.NumberOfFilesInSolution + ": " + fileInfo.Name);

        return fileModel;
    }
}