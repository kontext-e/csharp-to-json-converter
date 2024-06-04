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
        
        public List<FieldModel> Fields { get; set; } = [];
        public List<MethodModel> Methods { get; set; } = [];
        public List<PropertyModel> Properties { get; set; } = [];
        public List<ConstructorModel> Constructors { get; set; } = [];
        public List<string> ImplementedInterfaces { get; set; } = [];
    }
}