using System.Collections.Generic;

namespace csharp_to_json_converter.model
{
    public class ClassModel: AccessModifierModel
    {
        public string Name { get; set; }
        public string Fqn { get; set; }
        public string RelativePath { get; set; }
        public bool Abstract { get; set; }
        public string BaseType { get; set; }
        public string Md5 { get; set; }
        public bool Partial { get; set; }
        public List<string> ImplementedInterfaces { get; set; }
        public List<MethodModel> Methods { get; set; }
        public List<ConstructorModel> Constructors { get; set; }
        public List<FieldModel> Fields { get; set; }
        public List<PropertyModel> Properties { get; set; }

        public ClassModel()
        {
            Methods = new List<MethodModel>();
            Constructors = new List<ConstructorModel>();
            Fields = new List<FieldModel>();
            ImplementedInterfaces = new List<string>();
            Properties = new List<PropertyModel>();
        }
    }
}