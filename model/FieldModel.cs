using System.Collections.Generic;

namespace csharp_to_json_converter.model
{
    public class FieldModel
    {
        public string Name { get; set; }
        public string Fqn { get; set; }
        public string Type { get; set; }
        
        public bool Static { get; set; }
        public string Accessibility { get; set; }
        public bool Abstract { get; set; }
        public bool Sealed { get; set; }
        public bool Override { get; set; }
        public bool Virtual { get; set; }
        public bool Const { get; set; }
        public bool Volatile { get; set; }
        public string ConstantValue { get; set; }
    }
}