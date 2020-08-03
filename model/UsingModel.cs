using System.Text.Json.Serialization;

namespace csharp_to_json_converter.model
{
    public class UsingModel
    {
        [JsonPropertyName("static")] public bool StaticDirective { get; set; }

        public string Name { get; set; }

        public string Alias { get; set; }
    }
}