using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using NLog;

namespace csharp_to_json_converter.utils.ExtensionMethods;

public static class DocumentExtensions
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static readonly List<string> _excludedFiles = ["obj", "bin", ".nuget", "AssemblyAttributes", "AssemblyInfo"];

    public static bool IsEmpty(this Document document)
    {
        try
        {
            var fileIsEmpty = new StreamReader(document.FilePath!).ReadToEnd();
            return fileIsEmpty.Length == 0;
        }
        catch (IOException e)
        {
            Logger.Error("The file '{0}' could not be read:\n\n{1}", document.FilePath, e.Message);
        }

        return true;
    }

    public static bool IsExcluded(this Document document) 
        => _excludedFiles.All(file => document.FilePath!.Contains(file));
}