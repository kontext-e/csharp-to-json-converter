using System;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace csharp_to_json_converter.utils.ExtensionMethods;

public static class MethodSymbolExtensions
{
    public static string getFqn(this IMethodSymbol methodSymbol)
    {
        if (methodSymbol == null) { return string.Empty; }
            
        var regex = new Regex("<.*>");
        return regex.Replace(methodSymbol.ToString() ?? string.Empty, "<>", 99,methodSymbol!.ToString()!.IndexOf("(", StringComparison.Ordinal));
    }
}