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
        var compilation = project.GetCompilationAsync().Result;
        if (compilation == null) return projectModel;

        CheckForCompilationErrors(compilation);

        foreach (var fileInfo in project.Documents)
        {
            Logger.Debug("Analyzing script '{0}' ...", fileInfo.FilePath);
            projectModel.FileModels.Add(AnalyzeScript(fileInfo, compilation));
        }

        return projectModel;
    }

    private static void CheckForCompilationErrors(Compilation compilation)
    {
        foreach (var diagnostic in compilation.GetAllErrors())
        {
            Analyzer.HasErrors = true;
            Logger.Error(diagnostic.Descriptor.Description);
        }
    }

    private FileModel AnalyzeScript(Document fileInfo, Compilation compilation)
    {
        if (fileInfo.IsEmpty() || fileInfo.IsExcluded()) return null;

        var syntaxTree = compilation.SyntaxTrees.ToList().Find(syntaxTree => syntaxTree.FilePath.Equals(fileInfo.FilePath))!;
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        
        var fileAnalyzer = new FileAnalyzer(syntaxTree, semanticModel, _inputDirectory, _solution);
        var fileModel = fileAnalyzer.Analyze(fileInfo);
        Logger.Info("Analyzed File " + ++Analyzer.ScannedFiles + "/" + Analyzer.NumberOfFilesInSolution + ": " + fileInfo.Name);

        return fileModel;
    }
}