using System.Collections.Generic;

namespace csharp_to_json_converter.model
{
    public class InvocationModel
    {
        public int LineNumber { get; set; }
        public string MethodId { get; set; }
        public List<string> TypeArguments { get; set; } = new();
    }
}