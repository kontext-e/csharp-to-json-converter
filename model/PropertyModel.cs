using System.Collections.Generic;

namespace csharp_to_json_converter.model
{
    public class PropertyModel : AccessModifierModel
    {
        public string Name { get; set; }
        public string Fqn { get; set; }
        public IEnumerable<string> Type { get; set; }
        public bool Required { get; set; }
        public List<PropertyAccessorModel> Accessors { get; set; } = new();
        public List<InvocationModel> Accesses { get; set; } = new();
        public bool Readonly { get; set; }
    }
}
