using System.Collections.Generic;

namespace csharp_to_json_converter.model
{
    public class PropertyModel : AccessModifierModel
    {
        public string Name { get; set; }
        public string Fqn { get; set; }
        public string Type { get; set; }
        public List<PropertyAccessorModel> Accessors { get; set; } = new();

    }
}
