using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace csharp_to_json_converter.utils.ExtensionMethods;

public static class TypeSymbolResolveUtils
{
    #region PublicMethods

    public static List<ITypeSymbol> FindTypeArguemnts(this ISymbol symbol)
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

    public static IEnumerable<string> FindAllTypesRecursively(this ISymbol symbol, List<ITypeSymbol> typeArguments)
    {
        switch (symbol)
        {
            case INamedTypeSymbol namedTypeSymbol:
                return namedTypeSymbol.GetAllTypes(typeArguments);
            case ITypeParameterSymbol typeParameterSymbol:
                return typeParameterSymbol.GetAllTypes(typeArguments);
            case IArrayTypeSymbol arrayTypeSymbol:
                return arrayTypeSymbol.GetAllTypes(typeArguments);
            default:
                return new List<string>();
        }
    }

    #endregion

    #region Alanyzing Methods
    
    private static IEnumerable<string> GetAllTypes(this ITypeParameterSymbol typeParameterSymbol, List<ITypeSymbol> typeArguments)
    {
        var allTypes = new List<string>();

        var typeArgumentConstraints = LookUpTypeArgumentConstraints(typeArguments, typeParameterSymbol);
        if (typeArgumentConstraints.Length == 0)
        {
            allTypes.Add(typeParameterSymbol.Name);
            return allTypes;
        }
        
        foreach (var typeArgumentConstraint in typeArgumentConstraints)
        {
            allTypes.AddRange(FindAllTypesRecursively(typeArgumentConstraint, typeArguments));
        }
        
        return allTypes;
    }
    
    private static IEnumerable<string> GetAllTypes(this INamedTypeSymbol namedTypeSymbol, List<ITypeSymbol> typeArguments)
    {
        var allTypes = new List<string>();
        
        var nullableType = namedTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated;
        allTypes.Add(namedTypeSymbol.ConstructedFrom.ToDisplayString() + (nullableType ? "?" : ""));

        foreach (var typeArgument in namedTypeSymbol.TypeArguments)
        {
            allTypes.AddRange(FindAllTypesRecursively(typeArgument, typeArguments));
        }

        return allTypes;
    }
    
    private static IEnumerable<string> GetAllTypes(this IArrayTypeSymbol arrayTypeSymbol, List<ITypeSymbol> typeArguments)
    {
        return arrayTypeSymbol.ElementType.FindAllTypesRecursively(typeArguments).Append(arrayTypeSymbol.ToDisplayString());
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