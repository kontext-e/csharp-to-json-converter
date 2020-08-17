using System.Collections.Generic;

namespace csharp_to_json_converter.model
{
    public class ConstructorModel: MethodModel
    {
        public ConstructorModel()
        {
            Invocations = new List<InvokesModel>();
            Parameters = new List<ParameterModel>();
        }
    }
}