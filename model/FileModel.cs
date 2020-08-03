using System.Collections.Generic;

namespace csharp_to_json_converter.model
{
    public class FileModel
    {
        public string AbsolutePath { get; set; }
        public List<ClassModel> Classes { get; set; }
        public List<UsingModel> Usings { get; set; }

        internal FileModel()
        {
            Classes = new List<ClassModel>();
            Usings = new List<UsingModel>();
        }
    }
}