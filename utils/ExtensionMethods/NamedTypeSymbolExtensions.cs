using System.Collections.Generic;
using System.Linq;
using CommandLine;
using Microsoft.CodeAnalysis;

namespace csharp_to_json_converter.utils.ExtensionMethods;

public static class NamedTypeSymbolExtensions
{
    public static IEnumerable<string> GetAllTypes(this INamedTypeSymbol namedTypeSymbol)
    {
        var allTypes = new List<string>();
        var nullableType = namedTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated;
        allTypes.Add(namedTypeSymbol.ConstructedFrom.ToDisplayString() + (nullableType ? "?" : ""));

        foreach (var typeArgument in namedTypeSymbol.TypeArguments.Cast<INamedTypeSymbol>())
        {
            allTypes.AddRange(GetAllTypes(typeArgument));
        }

        return allTypes;
    }
}