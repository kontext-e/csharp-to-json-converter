using System.Collections.Generic;

namespace csharp_to_json_converter.model;

public class ProjectModel
{
    public string Name { get; set; }
    
    public string RelativePath { get; set; }
 
    public string AbsolutePath { get; set; }
    public List<FileModel> FileModels { get; set; } = [];
}