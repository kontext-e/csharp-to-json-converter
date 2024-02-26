using System.Collections.Generic;

namespace csharp_to_json_converter.model
{
    public class MethodModel: AccessModifierModel
    {
        public string Name { get; set; }
        public string Fqn { get; set; }
        public bool Abstract { get; set; }
        public bool Partial { get; set; }
        public string ReturnType { get; set; }
        public int CyclomaticComplexity { get; set; }
        public bool IsImplementation { get; set; }
        public bool IsExtensionMethod { get; set; }
        public string ExtendsType { get; set; }
        public List<InvocationModel> InvokedBy { get; set; }
        public List<ParameterModel> Parameters { get; set; }
        public string AssociatedProperty { get; set; }

        public MethodModel()
        {
            Parameters = new List<ParameterModel>();
            InvokedBy = new List<InvocationModel>();
        }
    }
}