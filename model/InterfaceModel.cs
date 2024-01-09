using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace csharp_to_json_converter.model
{
    public class InterfaceModel: ClassLikeModel
    {
        public string Name { get; set; }
        public string Fqn { get; set; }
        public string Md5 { get; set; }
        public string RelativePath { get; set; }
        
        // In Roslyn interface methods are "ConstructorDeclaration" nodes.
        // Internally we call them this way, but we do not want to communicate this to the outside.
        [JsonPropertyName("methods")] 
        public List<ConstructorModel> Constructors { get; set; } = new();
    }
}