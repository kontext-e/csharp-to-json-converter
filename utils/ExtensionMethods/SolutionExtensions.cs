using System.Linq;
using Microsoft.CodeAnalysis;

namespace csharp_to_json_converter.utils.ExtensionMethods;

public static class SolutionExtensions
{
    public static int CountSourceFiles(this Solution solution)
        =>  solution.Projects.Sum(project => project.Documents.Count(document => !document.IsExcluded()));
}