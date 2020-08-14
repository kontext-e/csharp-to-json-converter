using System.Collections.Generic;
using System.Linq;
using csharp_to_json_converter.model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace csharp_to_json_converter.utils.analyzers
{
    public class UsingsAnalyzer : AbstractAnalyzer
    {
        internal UsingsAnalyzer(SyntaxTree syntaxTree, SemanticModel semanticModel) : base(syntaxTree, semanticModel)
        {
        }

        internal void Analyze(FileModel fileModel)
        {
            List<UsingDirectiveSyntax> usingDirectiveSyntaxes = SyntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<UsingDirectiveSyntax>()
                .ToList();

            foreach (UsingDirectiveSyntax usingDirectiveSyntax in usingDirectiveSyntaxes)
            {
                UsingModel usingModel = new UsingModel();

                usingModel.Name = usingDirectiveSyntax.Name.ToString();

                NameEqualsSyntax nameEqualsSyntax =
                    usingDirectiveSyntax.DescendantNodes().OfType<NameEqualsSyntax>().FirstOrDefault();
                if (nameEqualsSyntax != null)
                {
                    usingModel.Alias = nameEqualsSyntax.Name.ToString();
                }

                if (usingDirectiveSyntax.StaticKeyword.Text != "")
                {
                    usingModel.StaticDirective = true;
                }

                fileModel.Usings.Add(usingModel);
            }
        }
    }
}