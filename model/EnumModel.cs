using System.Collections.Generic;

namespace csharp_to_json_converter.model
{
    public class EnumModel: AccessModifierModel
    {
        public string Name { get; set; }
        public string Fqn { get; set; }
        public string Md5 { get; set; }
        public string RelativePath { get; set; }
        public List<EnumMemberModel> Members { get; set; } = new();
    }
}