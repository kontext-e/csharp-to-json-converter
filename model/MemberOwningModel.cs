using System.Collections.Generic;

namespace csharp_to_json_converter.model
{
    public abstract class MemberOwningModel : AccessModifierModel
    {
        public bool Partial { get; set; }
        
        public string Name { get; set; }
        public string Fqn { get; set; }
        public string RelativePath { get; set; }
        public string Md5 { get; set; }
        
        public List<FieldModel> Fields { get; set; } = new();
        public List<MethodModel> Methods { get; set; } = new();
        public List<PropertyModel> Properties { get; set; } = new();
        public List<ConstructorModel> Constructors { get; set; } = new();
        public List<string> ImplementedInterfaces { get; set; } = new();
    }
}