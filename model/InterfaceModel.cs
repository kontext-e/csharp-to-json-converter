using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace csharp_to_json_converter.model
{
    public class InterfaceModel: ClassLikeModel
    {
        // In Roslyn interface methods are "ConstructorDeclaration" nodes.
        // Internally we call them this way, but we do not want to communicate this to the outside.
        [JsonPropertyName("interfaceMethods")] 
        public List<ConstructorModel> Constructors { get; set; } = new();
    }
}