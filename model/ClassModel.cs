﻿using System.Collections.Generic;

namespace csharp_to_json_converter.model
{
    public class ClassModel: ClassLikeModel
    {
        public string Name { get; set; }
        public string Fqn { get; set; }
        public string RelativePath { get; set; }
        public bool Abstract { get; set; }
        public string BaseType { get; set; }
        public string Md5 { get; set; }
        public bool Partial { get; set; }
    }
}