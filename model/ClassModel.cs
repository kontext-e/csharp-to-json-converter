using System.Collections.Generic;

namespace csharp_to_json_converter.model
{
    public class ClassModel: ClassLikeModel
    {
        public bool Abstract { get; set; }
        public string BaseType { get; set; }
        public bool Partial { get; set; }
    }
}