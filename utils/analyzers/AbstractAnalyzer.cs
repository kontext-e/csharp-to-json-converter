using System.Text;
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
        
        protected string BuildMD5(string input)
        {
            using System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
                
            StringBuilder sb = new StringBuilder();
            foreach (byte t in hashBytes)
            {
                sb.Append(t.ToString("X2"));
            }
            return sb.ToString();
        }
    }
}