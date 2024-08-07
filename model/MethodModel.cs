﻿using System.Collections.Generic;

namespace csharp_to_json_converter.model
{
    public class MethodModel: AccessModifierModel
    {
        public string Name { get; set; }
        public string Fqn { get; set; }
        public bool Abstract { get; set; }
        public bool Partial { get; set; }
        public IEnumerable<string> ReturnTypes { get; set; }
        public int CyclomaticComplexity { get; set; }
        public bool IsImplementation { get; set; }
        public bool IsExtensionMethod { get; set; }
        public string ExtendsType { get; set; }
        public List<InvocationModel> InvokedBy { get; set; } = new();
        public List<ParameterModel> Parameters { get; set; } = new();
        public List<ArrayCreationModel> CreatesArrays { get; set; } = new();
        public string AssociatedProperty { get; set; }
    }
}