﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using csharp_to_json_converter.model;
using csharp_to_json_converter.utils.analyzers;
using csharp_to_json_converter.utils.ExtensionMethods;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using NLog;

namespace csharp_to_json_converter.utils
{
    public class Analyzer(DirectoryInfo inputDirectory)
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        internal static Dictionary<string, Compilation> Compilations;

        private readonly List<ProjectModel> _projectModels = [];
        private Solution _solution;
        internal static bool HasErrors;
        internal static int NumberOfFilesInSolution = 0;
        internal static int ScannedFiles = 0;

        internal List<ProjectModel> Analyze()
        {
            Logger.Info("Opening solution ...");
            OpenSolution();
            Logger.Info("Finished opening solution.");
            
            Logger.Info("Compiling projects");
            CompileProjects();
            Logger.Info("Finished compiling projects");
            
            Logger.Info("Analyzing scripts ...");
            AnalyzeProjects();
            Logger.Info("Finished analyzing scripts.");

            if (HasErrors) Logger.Warn("Scan and Analysis will be flawed if Compilation has Errors. It is highly reccomended to fix all Errors");
            
            return _projectModels;
        }

        private void CompileProjects()
        {
            foreach (var project in _solution.Projects)
            {
                var compilation = project.GetCompilationAsync().Result;
                if (compilation == null) return;

                CheckForCompilationErrors(compilation);

                Compilations[project.Name] = compilation;
            }
        }
        
        private static void CheckForCompilationErrors(Compilation compilation)
        {
            foreach (var diagnostic in compilation.GetAllErrors())
            {
                HasErrors = true;
                Logger.Error(diagnostic.ToString);
            }
        }

        private void OpenSolution()
        {
            try
            {
                var solutionFile = ScriptFinder.FindSolutionFile(inputDirectory);
                MSBuildLocator.RegisterDefaults();
                var workspace = MSBuildWorkspace.Create();
                _solution = workspace.OpenSolutionAsync(solutionFile.FullName).Result;
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
                var projectAnalyzer = new ProjectAnalyzer(inputDirectory, _solution);
                _projectModels.Add(projectAnalyzer.Analyze(project));
                Logger.Info("Analyzed Project " + ++scannedProjects + "/" + _solution.Projects.Count() + ": " + project.Name);
            }
        }
        
    }
}