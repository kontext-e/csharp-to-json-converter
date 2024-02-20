using System.IO;
using System.Linq;
using NLog;

namespace csharp_to_json_converter.utils
{
    public static class ScriptFinder
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static FileInfo FindSolutionFile(DirectoryInfo inputDirectory)
        {
            var fileInfos = inputDirectory.GetFiles("*.sln").ToList();
            if (fileInfos.Count == 1)
            {
                Logger.Debug("Fund Solution File: " + fileInfos[0].FullName);
                return fileInfos[0];
            }

            Logger.Error("Please make sure there is exactly one solution file in Directory");
            return null;
        }
    }
}