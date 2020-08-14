using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;

namespace csharp_to_json_converter.utils
{
    public static class ScriptFinder
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        internal static List<FileInfo> FindScriptsRecursivelyUnder(DirectoryInfo directoryInfo)
        {
            List<FileInfo> fileInfos = TryToFindScriptsIn(directoryInfo);

            foreach (FileInfo fi in fileInfos)
            {
                Logger.Debug("Found script: " + fi.FullName);
            }

            DirectoryInfo[] subDirs = directoryInfo.GetDirectories();

            foreach (DirectoryInfo dirInfo in subDirs)
            {
                if (directoryInfo.Name.Equals("obj") || directoryInfo.Name.Equals("bin"))
                {
                    continue;
                }
                
                fileInfos.AddRange(FindScriptsRecursivelyUnder(dirInfo));
            }

            return fileInfos;
        }

        private static List<FileInfo> TryToFindScriptsIn(DirectoryInfo directoryInfo)
        {
            List<FileInfo> fileInfos = new List<FileInfo>();
            try
            {
                fileInfos = directoryInfo.GetFiles("*.cs").ToList();
            }
            catch (UnauthorizedAccessException e)
            {
                Logger.Error(e.Message);
            }
            catch (DirectoryNotFoundException e)
            {
                Logger.Error(e.Message);
            }

            return fileInfos;
        }
    }
}