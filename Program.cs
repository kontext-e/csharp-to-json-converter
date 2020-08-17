using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CommandLine;
using csharp_to_json_converter.model;
using csharp_to_json_converter.utils;
using NLog;

namespace csharp_to_json_converter
{
    static class Program
    {
        private static JsonSerializerOptions _options = new JsonSerializerOptions
        {
            IgnoreNullValues = true,
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private const int ErrorInvalidCommandLine = 0x667;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        static void Main(string[] args)
        {
            NLogConfiguration.Configure(LogLevel.Info);

            ParserResult<CommandLineArguments> parserResult = Parser.Default.ParseArguments<CommandLineArguments>(args);

            if (parserResult.Tag == ParserResultType.NotParsed)
            {
                Environment.ExitCode = ErrorInvalidCommandLine;
                return;
            }

            DirectoryInfo inputDirectory = null;
            DirectoryInfo outputDirectory = null;

            parserResult.WithParsed((commandLineArguments) =>
            {
                inputDirectory = new DirectoryInfo(commandLineArguments.InputDirectory);
                outputDirectory = new DirectoryInfo(commandLineArguments.OutputDirectory);
            });

            Logger.Info("Searching for scripts in '{0}'.", inputDirectory.FullName);
            List<FileInfo> fileInfos = ScriptFinder.FindScriptsRecursivelyUnder(inputDirectory);
            Logger.Info("Found '{0}' scripts in '{1}'.", fileInfos.Count, inputDirectory.FullName);

            Analyzer analyzer = new Analyzer(fileInfos, inputDirectory);

            List<FileModel> fileModelList = analyzer.Analyze();

            Logger.Info("Writing model to JSON in '{0}' ...", outputDirectory.FullName);

            foreach (FileModel fileModel in fileModelList)
            {
                DirectoryInfo subFolderInOutputDirectory =
                    CreateSubFolderInOutputDirectory(fileModel, inputDirectory, outputDirectory);

                string jsonString = JsonSerializer.Serialize(fileModel, _options);
                File.WriteAllText(BuildJsonName(subFolderInOutputDirectory, fileModel), jsonString);
            }

            Logger.Info("Finished writing model to JSON.");
        }

        private static DirectoryInfo CreateSubFolderInOutputDirectory(FileModel fileModel, DirectoryInfo inputDirectory,
            DirectoryInfo outputDirectory)
        {
            DirectoryInfo parent = Directory.GetParent(fileModel.AbsolutePath);
            string relativeDirectoryPath = Path.GetRelativePath(inputDirectory.FullName, parent.FullName);
            string absoluteDirectoryPath =
                Path.Combine(outputDirectory.FullName, inputDirectory.Name, relativeDirectoryPath);
            DirectoryInfo subFolderOfOutputDirectory = Directory.CreateDirectory(absoluteDirectoryPath);
            return subFolderOfOutputDirectory;
        }

        private static string BuildJsonName(DirectoryInfo outputDirectory, FileModel fileModel)
        {
            string fileName = fileModel.Name + ".json";
            return Path.Combine(outputDirectory.FullName, fileName);
        }
    }
}