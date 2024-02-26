using System.Collections.Generic;

namespace csharp_to_json_converter.model
{
    public class ConstructorModel: MethodModel
    {
        public bool IsPrimaryConstructor { get; set; }
        public ConstructorModel()
        {
            Parameters = new List<ParameterModel>();
        }
    }
}