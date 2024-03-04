using Microsoft.CodeAnalysis;

namespace csharp_to_json_converter.utils.ExtensionMethods;

public static class SemanticModelExtensions
{
    public static ISymbol FindSymbolInfoOrUseSingleCandidate(this SemanticModel semanticModel, SyntaxNode syntaxNode)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(syntaxNode);
        return symbolInfo is { CandidateReason: CandidateReason.OverloadResolutionFailure, CandidateSymbols.Length: 1, Symbol: null }
            ? symbolInfo.CandidateSymbols[0]
            : symbolInfo.Symbol;
    }

}