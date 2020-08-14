using Microsoft.CodeAnalysis;

namespace csharp_to_json_converter.utils.analyzers
{
    public abstract class AbstractAnalyzer
    {
        protected readonly SyntaxTree SyntaxTree;
        protected readonly SemanticModel SemanticModel;

        protected AbstractAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel)
        {
            SyntaxTree = syntaxTree;
            SemanticModel = semanticModel;
        }
    }
}