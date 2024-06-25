using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace csharp_to_json_converter.utils.ExtensionMethods;

public static class TypeSymbolResolveUtils
{
    #region PublicMethods

    public static List<ITypeSymbol> FindTypeArguments(this ISymbol symbol)
    {
        switch (symbol)
        {
            case IMethodSymbol methodSymbol:
                return FingMethodTypeArguments(symbol, methodSymbol);
            case IParameterSymbol parameterSymbol:
                if (FindParameterTypeArguments(parameterSymbol, out var typeSymbols)) 
                    return typeSymbols;
                break;
            case IFieldSymbol or IPropertySymbol:
                return symbol.ContainingType.TypeArguments.ToList();
            default:
                return [];
        }

        return [];
    }

    public static IEnumerable<string> FindAllTypes(this ISymbol symbol, List<ITypeSymbol> typeArguments)
    {
        var result = new List<string>();
        FindAllTypesRecursively(symbol, typeArguments, result);
        return result;
    }
    
    #endregion

    private static void FindAllTypesRecursively(this ISymbol symbol, List<ITypeSymbol> typeArguments, List<string> result)
    {
        switch (symbol)
        {
            case INamedTypeSymbol namedTypeSymbol:
                namedTypeSymbol.GetAllTypes(typeArguments, result); break;
            case ITypeParameterSymbol typeParameterSymbol:
                typeParameterSymbol.GetAllTypes(typeArguments, result); break;
            case IArrayTypeSymbol arrayTypeSymbol:
                arrayTypeSymbol.GetAllTypes(typeArguments, result); break;
        }
    }

    #region Analyzing Methods

    private static void GetAllTypes(this ITypeParameterSymbol typeParameterSymbol, List<ITypeSymbol> typeArguments, List<string> result)
    {
        var typeArgumentConstraints = LookUpTypeArgumentConstraints(typeArguments, typeParameterSymbol);
        if (typeArgumentConstraints.Length == 0)
        {
            result.Add(typeParameterSymbol.Name);
        }
        
        foreach (var typeArgumentConstraint in typeArgumentConstraints)
        {
            if (result.Contains(((INamedTypeSymbol)typeArgumentConstraint).ConstructedFrom.ToDisplayString())) { continue; }
            result.Add(typeParameterSymbol.ToDisplayString());
            FindAllTypesRecursively(typeArgumentConstraint, typeArguments, result);
        }
    }
    
    private static void GetAllTypes(this INamedTypeSymbol namedTypeSymbol, List<ITypeSymbol> typeArguments, List<string> result)
    {
        var nullableType = namedTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated;
        result.Add(namedTypeSymbol.ConstructedFrom.ToDisplayString() + (nullableType ? "?" : ""));

        foreach (var typeArgument in namedTypeSymbol.TypeArguments)
        {
            if (result.Contains(typeArgument.ToDisplayString())) { continue; }
            FindAllTypesRecursively(typeArgument, typeArguments, result);
        }
    }
    
    private static void GetAllTypes(this IArrayTypeSymbol arrayTypeSymbol, List<ITypeSymbol> typeArguments, List<string> result)
    {
        result.AddRange(arrayTypeSymbol.ElementType.FindAllTypes(typeArguments).Append(arrayTypeSymbol.ToDisplayString()));
    }

    #endregion

    #region Helpers

    private static ImmutableArray<ITypeSymbol> LookUpTypeArgumentConstraints(List<ITypeSymbol> typeArguments, ITypeSymbol typeArgument)
    {
        var typeArgumentSymbol = (ITypeParameterSymbol) typeArguments.Find(t => t.MetadataName == typeArgument.MetadataName);
        var constraintTypes = typeArgumentSymbol?.ConstraintTypes ?? [];
        return constraintTypes;
    }
    
    private static bool FindParameterTypeArguments(IParameterSymbol parameterSymbol, out List<ITypeSymbol> typeSymbols)
    {
        var containingMethodSymbol = (IMethodSymbol)parameterSymbol.ContainingSymbol;
        if (containingMethodSymbol.ContainingSymbol is not INamedTypeSymbol containingTypeSymbol)
        {
            typeSymbols = [];
            return false;
        }
        var typeArguments = containingMethodSymbol.TypeArguments.ToList();
        typeArguments.AddRange(containingTypeSymbol.TypeArguments);
        typeSymbols = typeArguments;
        return true;
    }

    private static List<ITypeSymbol> FingMethodTypeArguments(ISymbol symbol, IMethodSymbol methodSymbol)
    {
        var methodTypeArguments = methodSymbol.TypeArguments.ToList();
        methodTypeArguments.AddRange(symbol.ContainingType.TypeArguments);
        return methodTypeArguments;
    }
    
    #endregion
    
}