using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace csharp_to_json_converter.utils.ExtensionMethods;

public static class TypeSymbolResolveUtils
{
    
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

        var typeArguments = symbol.ContainingType.TypeArguments.ToList();

        foreach (var typeArgument in namedTypeSymbol.TypeArguments)
        {
            if (TypeArgumentIsFromContainingGenericType(typeArguments, typeArgument))
            {
                foreach (var constraintType in LookUpTypeArgumentConstraints(typeArguments, typeArgument))
                {
                    allTypes.AddRange(GetAllTypes((INamedTypeSymbol) constraintType, symbol));
                }
            }
            else
            {
                allTypes.AddRange(GetAllTypes((INamedTypeSymbol) typeArgument, symbol));
            }
        }

        return allTypes;
    }

    //  
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
}