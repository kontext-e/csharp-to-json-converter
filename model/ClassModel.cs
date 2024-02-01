namespace csharp_to_json_converter.model
{
    public class ClassModel: MemberOwningModel
    {
        public bool Abstract { get; set; }
        public string BaseType { get; set; }
    }
}