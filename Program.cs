using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml.Linq;
using csharp_to_json_converter.model;
using csharp_to_json_converter.utils;
using Microsoft.CodeAnalysis;
using NLog;

namespace csharp_to_json_converter
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            NLogConfiguration.Configure(LogLevel.Info);

            if (args.Length == 0)
            {
                Logger.Error("Plase specify the input directory.");
                return;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(args[0]);

            Logger.Info("Searching for scripts in '{0}'.", directoryInfo.FullName);
            List<FileInfo> fileInfos = ScriptFinder.FindScriptsRecursivelyUnder(directoryInfo);
            Logger.Info("Found '{0}' scripts in '{1}'.", fileInfos.Count, directoryInfo.FullName);

            Analyzer analyzer = new Analyzer(fileInfos);

            XDocument xDocument = analyzer.Analyze();
            
            Logger.Info("Writing model to XML ...");
            xDocument.Save("output.xml");
            Logger.Info("Finished writing model to XML.");
        }
    }
}