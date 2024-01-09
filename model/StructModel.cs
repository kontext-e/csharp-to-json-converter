using System.Collections.Generic;

namespace csharp_to_json_converter.model
{
    public class StructModel : ClassLikeModel
    {
        public string Name { get; set; }
        public string Fqn { get; set; }
        public string RelativePath { get; set; }
        public bool Abstract { get; set; }
        public string BaseType { get; set; }
        public string Md5 { get; set; }
        public bool Partial { get; set; }
        public List<string> ImplementedInterfaces { get; set; } = new();
        public List<MethodModel> Methods { get; set; } = new();
        public List<ConstructorModel> Constructors { get; set; } = new();
        public List<FieldModel> Fields { get; set; } = new();
        public List<PropertyModel> Properties { get; set; } = new();
    }
}