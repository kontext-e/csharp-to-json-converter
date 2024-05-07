using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace csharp_to_json_converter.utils.ExtensionMethods;

public static class TypeSymbolResolveUtils
{

    #region Entrypoints for TypeAnalysis

    // Entrypoint to "GetAllTypes(this INamedTypeSymbol namedTypeSymbol, ISymbol symbol)"
    // for when Main type of member is Array of Generic type,
    // I.e. if:
    // public class foo<T> { private T[] bar { get; set; } }
    public static IEnumerable<string> GetAllTypes(this IArrayTypeSymbol arrayTypeSymbol, ISymbol symbol)
    {
        return AnalyzeArrayTypes(symbol, arrayTypeSymbol);
    }
    
    // Entrypoint to "GetAllTypes(this INamedTypeSymbol namedTypeSymbol, ISymbol symbol)" for when Main type of member is Generic type,
    // I.e. if:
    // public class foo<T> { private T bar { get; set; } }
    public static IEnumerable<string> GetAllTypes(this ITypeParameterSymbol typeParameterSymbol, ISymbol symbol)
    {
        var allTypes = new List<string>();
        if (symbol is not IFieldSymbol or IPropertySymbol) return allTypes;

        var containingType = symbol.ContainingType.TypeArguments.ToList();
        foreach (var typeArgumentConstraint in LookUpTypeArgumentConstraints(containingType, typeParameterSymbol))
        {
             allTypes.AddRange(((INamedTypeSymbol)typeArgumentConstraint).GetAllTypes(symbol));
        }

        return allTypes;
    }

    // Recursive Logic to find all Types of a property or field.
    // string --> string
    // List<string> --> List<T>, string
    // Entrypoint for when Main Type of member is an explicit Type
    public static IEnumerable<string> GetAllTypes(this INamedTypeSymbol namedTypeSymbol, ISymbol symbol)
    {
        var allTypes = new List<string>();
        if (symbol is not IFieldSymbol and not IPropertySymbol) return allTypes;
        
        var nullableType = namedTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated;
        allTypes.Add(namedTypeSymbol.ConstructedFrom.ToDisplayString() + (nullableType ? "?" : ""));

        allTypes.AddRange(AnalyzeNestedTypes(namedTypeSymbol, symbol));

        return allTypes;
    }

    #endregion

    #region Analyzers for different Type Declarations

    private static List<string> AnalyzeNestedTypes(INamedTypeSymbol namedTypeSymbol, ISymbol symbol)
    {
        var allTypes = new List<string>();
        var typeArguments = symbol.ContainingType.TypeArguments.ToList();
        
        foreach (var typeArgument in namedTypeSymbol.TypeArguments)
        {
            // Case of TypeArgument
            if (TypeArgumentIsFromContainingGenericType(typeArguments, typeArgument))
            {
                allTypes.AddRange(AnalyzeGenericTypes(symbol, typeArgument, typeArguments));
            }
            // Case of ArrayType
            else if (typeArgument is IArrayTypeSymbol arrayType)
            {
                allTypes.AddRange(AnalyzeArrayTypes(symbol, arrayType));
            }
            // Case of 'Normal' Type
            else
            {
                allTypes.AddRange(GetAllTypes((INamedTypeSymbol)typeArgument, symbol));
            }
        }

        return allTypes;
    }

    private static List<string> AnalyzeArrayTypes(ISymbol symbol, IArrayTypeSymbol arrayType)
    {
        var allTypes = new List<string>();
        var typeArguments = symbol.ContainingType.TypeArguments.ToList();
        switch (arrayType.ElementType)
        {
            case INamedTypeSymbol:
                allTypes.Add(arrayType.ToDisplayString());
                break;
            case ITypeParameterSymbol:
                allTypes.AddRange(AnalyzeGenericTypes(symbol, arrayType, typeArguments));
                break;
        }

        return allTypes;
    }

    private static List<string> AnalyzeGenericTypes(ISymbol symbol, ITypeSymbol typeSymbol, List<ITypeSymbol> typeArguments)
    {
        var allTypes = new List<string>();

        var type = typeSymbol is IArrayTypeSymbol arrayTypeSymbol ? arrayTypeSymbol.ElementType : typeSymbol;
        foreach (var constraintType in LookUpTypeArgumentConstraints(typeArguments, type))
        {
            allTypes.AddRange(GetAllTypes((INamedTypeSymbol)constraintType, symbol));
        }

        return allTypes;
    }

    #endregion

    #region Helpers

    private static ImmutableArray<ITypeSymbol> LookUpTypeArgumentConstraints(List<ITypeSymbol> typeArguments, ITypeSymbol typeArgument)
    {
        var typeArgumentSymbol = (ITypeParameterSymbol) typeArguments.Find(t => t.MetadataName == typeArgument.MetadataName);
        var constraintTypes = typeArgumentSymbol.ConstraintTypes;
        return constraintTypes;
    }

    private static bool TypeArgumentIsFromContainingGenericType(List<ITypeSymbol> typeArguments, ITypeSymbol typeArgument)
    {
        return typeArguments.Select(argument => argument.MetadataName).Contains(typeArgument.MetadataName);
    }

    #endregion
}