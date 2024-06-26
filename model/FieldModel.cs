﻿using System.Collections.Generic;

namespace csharp_to_json_converter.model
{
    public class FieldModel : AccessModifierModel
    {
        public string Name { get; set; }
        public string Fqn { get; set; }
        public IEnumerable<string> Types { get; set; }
        public bool Abstract { get; set; }
        public bool Const { get; set; }
        public bool Volatile { get; set; }
        public bool Required { get; set; }
        public string ConstantValue { get; set; }
    }
}