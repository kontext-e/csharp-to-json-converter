using System.IO;
using System.Linq;
using NLog;

namespace csharp_to_json_converter.utils;

public static class ScriptFinder
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static FileInfo FindSolutionFile(DirectoryInfo inputDirectory)
    {
        var findSolutionFile = FindSolutionFileRecursively(inputDirectory);
        if (findSolutionFile == null)
            throw new FileNotFoundException("Could not find Solution File in Directory (including nested): " + inputDirectory.FullName);
        
        return findSolutionFile;
    }

    private static FileInfo FindSolutionFileRecursively(DirectoryInfo inputDirectory)
    {
        var fileInfos = inputDirectory.GetFiles("*.sln").ToList();
        if (fileInfos.Count == 1)
        {
            Logger.Debug("Found Solution File: " + fileInfos[0].FullName);
            return fileInfos[0];
        }

        return inputDirectory.GetDirectories().Select(FindSolutionFileRecursively).FirstOrDefault();
    }
}