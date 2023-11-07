using System.Collections.Generic;

namespace csharp_to_json_converter.model
{
    public class ConstructorModel: MethodModel
    {
        public ConstructorModel()
        {
            MemberAccesses = new List<MemberAccessModel>();
            Parameters = new List<ParameterModel>();
        }
    }
}