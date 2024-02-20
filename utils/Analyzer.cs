using System.Collections.Generic;
using System.IO;
using csharp_to_json_converter.model;
using csharp_to_json_converter.utils.analyzers;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using NLog;

namespace csharp_to_json_converter.utils
{
    public class Analyzer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private List<FileModel> _fileModels;
        private Dictionary<string, SyntaxTree> _syntaxTrees;
        private Dictionary<string, Compilation> _compilation;
        private DirectoryInfo _inputDirectory;
        private Solution _solution;

        public Analyzer(DirectoryInfo inputDirectory)
        {
            _inputDirectory = inputDirectory;
            _fileModels = new List<FileModel>();
            _syntaxTrees = new Dictionary<string, SyntaxTree>();
            _compilation = new Dictionary<string, Compilation>();
        }

        internal List<FileModel> Analyze()
        {
            Logger.Info("Creating compilation ...");
            CreateCompilation();
            Logger.Info("Finished creating compilation.");
            
            Logger.Info("Analyzing scripts ...");
            AnalyzeScripts();
            Logger.Info("Finished analyzing scripts.");

            return _fileModels;
        }

        private void CreateCompilation()
        {
            OpenSolution();
            ReadProjects();
        }

        private void OpenSolution()
        {
            var solutionFile = ScriptFinder.FindSolutionFile(_inputDirectory);
            MSBuildLocator.RegisterDefaults();
            var workspace = MSBuildWorkspace.Create();
            _solution = workspace.OpenSolutionAsync(solutionFile.FullName).Result;
        }

        private void ReadProjects()
        {
            foreach (var project in _solution.Projects)
            {
                var compilation = project.GetCompilationAsync().Result;
                if (compilation == null) continue;
                
                _compilation[project.FilePath!] = compilation;
                ReadSyntaxTrees(compilation);
            }
        }

        private void ReadSyntaxTrees(Compilation compilation)
        {
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var filePath = syntaxTree.FilePath;
                if (filePath.Contains(".nuget") || filePath.Contains("obj") || filePath.Contains("bin")) continue;
                _syntaxTrees.TryAdd(filePath, syntaxTree);
            }
        }

        private void AnalyzeScripts()
        {
            foreach (var project in _solution.Projects) 
            {
                foreach (var fileInfo in project.Documents) 
                {
                    if (fileInfo.FilePath!.Contains("obj") || fileInfo.FilePath.Contains("bin") || fileInfo.FilePath.Contains(".nuget")) continue;
                    AnalyzeScript(project, fileInfo);
                }
            }
        }

        private void AnalyzeScript(Project project, Document fileInfo)
        {
            Logger.Debug("Analyzing script '{0}' ...", fileInfo.FilePath);
            if (FileIsEmpty(fileInfo)) return;
            
            SyntaxTree syntaxTree = _syntaxTrees[fileInfo.FilePath!];
            SemanticModel semanticModel = _compilation[project.FilePath!].GetSemanticModel(syntaxTree);
            
            FileAnalyzer fileAnalyzer = new FileAnalyzer(syntaxTree, semanticModel, _inputDirectory);
            _fileModels.Add(fileAnalyzer.Analyze(fileInfo));
        }
        
        private static bool FileIsEmpty(Document fileInfo)
        {
            try
            {
                using StreamReader sr = new StreamReader(fileInfo.FilePath!);
                var fileIsEmpty = sr.ReadToEnd();
                return fileIsEmpty.Length == 0;
            }
            catch (IOException e)
            {
                Logger.Error("The file '{0}' could not be read:\n\n{1}", fileInfo.FilePath, e.Message);
            }

            return true;
        }
    }
}