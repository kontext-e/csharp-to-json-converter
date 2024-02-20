using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommandLine;
using csharp_to_json_converter.model;
using csharp_to_json_converter.utils;
using NLog;

namespace csharp_to_json_converter
{
    static class Program
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private const int ErrorInvalidCommandLine = 0x667;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        static void Main(string[] args)
        {
            if (SetupEnvironment(args, out var parserResult)) return;
            var (inputDirectory, outputDirectory) = PrepareIO(parserResult);
            var fileModelList = AnalyzeFiles(inputDirectory);
            WriteFiles(outputDirectory, fileModelList, inputDirectory);
        }

        private static (DirectoryInfo inputDirectory, DirectoryInfo outputDirectory) PrepareIO(ParserResult<CommandLineArguments> parserResult)
        {
            DirectoryInfo inputDirectory = null;
            DirectoryInfo outputDirectory = null;

            parserResult.WithParsed((commandLineArguments) =>
            {
                inputDirectory = new DirectoryInfo(commandLineArguments.InputDirectory);
                outputDirectory = new DirectoryInfo(commandLineArguments.OutputDirectory);
            });
            return (inputDirectory, outputDirectory);
        }

        private static bool SetupEnvironment(string[] args, out ParserResult<CommandLineArguments> parserResult)
        {
            NLogConfiguration.Configure(LogLevel.Info);
            
            parserResult = Parser.Default.ParseArguments<CommandLineArguments>(args);
            if (parserResult.Tag != ParserResultType.NotParsed) return false;
            Environment.ExitCode = ErrorInvalidCommandLine;
            
            return true;
        }

        private static void WriteFiles(DirectoryInfo outputDirectory, List<FileModel> fileModelList, DirectoryInfo inputDirectory)
        {
            Logger.Info("Writing model to JSON in '{0}' ...", outputDirectory.FullName);
            WriteModelToJson(fileModelList, inputDirectory, outputDirectory);
            Logger.Info("Finished writing model to JSON.");
        }

        private static List<FileModel> AnalyzeFiles(DirectoryInfo inputDirectory)
        {
            Analyzer analyzer = new Analyzer(inputDirectory);
            List<FileModel> fileModelList = analyzer.Analyze();
            return fileModelList;
        }

        private static void WriteModelToJson(List<FileModel> fileModelList, DirectoryInfo inputDirectory, DirectoryInfo outputDirectory)
        {
            foreach (var fileModel in fileModelList)
            {
                var subFolderInOutputDirectory = CreateSubFolderInOutputDirectory(fileModel, inputDirectory, outputDirectory);
                var jsonString = JsonSerializer.Serialize(fileModel, JsonSerializerOptions);
                File.WriteAllText(BuildJsonName(subFolderInOutputDirectory, fileModel), jsonString);
            }
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