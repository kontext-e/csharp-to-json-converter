using System.Collections.Generic;

namespace csharp_to_json_converter.model
{
    public class FileModel
    {
        public string Name { get; set; }
        public string AbsolutePath { get; set; }
        public string RelativePath { get; set; }
        public List<ClassModel> Classes { get; set; }
        public List<UsingModel> Usings { get; set; }
        public List<EnumModel> Enums { get; set; }
        public List<InterfaceModel> Interfaces { get; set; }
        
        public List<StructModel> Structs { get; set; }

        internal FileModel()
        {
            Classes = new List<ClassModel>();
            Usings = new List<UsingModel>();
            Enums = new List<EnumModel>();
            Interfaces = new List<InterfaceModel>();
            Structs = new List<StructModel>();
        }
    }
}