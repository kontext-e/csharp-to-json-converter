using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharp_to_json_converter.model
{
    public class PropertyModel : AccessModifierModel
    {
        public string Name { get; set; }
        public string Fqn { get; set; }
        public string Type { get; set; }
        public List<string> Accessors { get; set; } = new List<string>();

    }
}
