using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using csharp_to_json_converter.model;
using csharp_to_json_converter.utils.analyzers;
using csharp_to_json_converter.utils.ExtensionMethods;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using NLog;

namespace csharp_to_json_converter.utils
{
    public class Analyzer(DirectoryInfo solutionFile)
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        internal static readonly Dictionary<string, Compilation> Compilations = new();
        private static readonly Dictionary<SyntaxTree, Compilation> CompilationBySyntaxTree = new();

        private readonly List<ProjectModel> _projectModels = [];
        private Solution _solution;
        private static bool _hasErrors;
        internal static int NumberOfFilesInSolution = 0;
        internal static int ScannedFiles = 0;

        // NOTE: Caller in Program.cs needs to be updated to:
        //   var results = await analyzer.AnalyzeAsync();
        // or if Main is not async yet:
        //   var results = analyzer.AnalyzeAsync().GetAwaiter().GetResult();
        internal async Task<List<ProjectModel>> AnalyzeAsync()
        {
            Logger.Info("Opening solution ...");
            await OpenSolutionAsync();
            Logger.Info("Finished opening solution.");

            Logger.Info("Compiling projects");
            await CompileProjectsAsync();
            Logger.Info("Finished compiling projects");

            Logger.Info("Analyzing scripts ...");
            AnalyzeProjects();
            Logger.Info("Finished analyzing scripts.");

            if (_hasErrors) Logger.Warn("Scan and Analysis will be flawed if Compilation has Errors. It is highly reccomended to fix all Errors");

            return _projectModels;
        }

        private async Task CompileProjectsAsync()
        {
            var graph = _solution.GetProjectDependencyGraph();
            var sortedIds = graph.GetTopologicallySortedProjects().ToList();

            // Kick off all compilations without awaiting individually —
            // lets independent branches in the dependency graph run in parallel
            var tasks = sortedIds.ToDictionary(
                id => id,
                id => _solution.GetProject(id)!.GetCompilationAsync()
            );

            foreach (var id in sortedIds)
            {
                var compilation = await tasks[id];
                if (compilation == null) continue;

                CheckForCompilationErrors(compilation);

                var project = _solution.GetProject(id)!;
                Compilations[project.Name] = compilation;

                // Build reverse lookup: SyntaxTree → Compilation
                // so FindSemanticModelForFileContainingSyntaxNode is O(1) instead of O(projects × files)
                foreach (var tree in compilation.SyntaxTrees)
                {
                    CompilationBySyntaxTree[tree] = compilation;
                }
            }
        }

        private static void CheckForCompilationErrors(Compilation compilation)
        {
            foreach (var diagnostic in compilation.GetAllErrors())
            {
                _hasErrors = true;
                Logger.Error(diagnostic.ToString);
            }
        }

        private async Task OpenSolutionAsync()
        {
            try
            {
                MSBuildLocator.RegisterDefaults();
                var workspace = MSBuildWorkspace.Create();
                _solution = await workspace.OpenSolutionAsync(solutionFile.FullName);
                NumberOfFilesInSolution = _solution.CountSourceFiles();
            }
            catch (FileNotFoundException e)
            {
                Logger.Error(e);
            }
        }

        private void AnalyzeProjects()
        {
            var scannedProjects = 0;
            foreach (var project in _solution.Projects)
            {
                var projectAnalyzer = new ProjectAnalyzer(solutionFile, _solution);
                _projectModels.Add(projectAnalyzer.Analyze(project));
                Logger.Info("Analyzed Project " + ++scannedProjects + "/" + _solution.Projects.Count() + ": " + project.Name);
            }
        }

        // O(1) lookup via pre-built reverse index
        internal static SemanticModel FindSemanticModelForFileContainingSyntaxNode(SyntaxNode syntaxNode)
        {
            if (CompilationBySyntaxTree.TryGetValue(syntaxNode.SyntaxTree, out var compilation))
            {
                return compilation.GetSemanticModel(syntaxNode.SyntaxTree);
            }
            throw new Exception("Analyzed file does not belong to solution");
        }
    }
}