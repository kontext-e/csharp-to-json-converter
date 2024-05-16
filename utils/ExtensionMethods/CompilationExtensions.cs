using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Linq;


namespace csharp_to_json_converter.utils.ExtensionMethods;

public static class CompilationExtensions
{
    public static IEnumerable<Diagnostic> GetAllErrors(this Compilation compilation) 
        => compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity >= DiagnosticSeverity.Error);
}