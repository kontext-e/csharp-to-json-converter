using System.Collections.Generic;

namespace csharp_to_json_converter.model
{
    public class EnumModel
    {
        public string Name { get; set; }
        public string Fqn { get; set; }
        public string Md5 { get; set; }
        public List<EnumMemberModel> Members { get; set; }

        public EnumModel()
        {
            Members = new List<EnumMemberModel>();
        }
    }
}