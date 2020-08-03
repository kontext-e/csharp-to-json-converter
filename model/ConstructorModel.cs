namespace csharp_to_json_converter.model
{
    public class ConstructorModel
    {
        public string Name { get; set; }
        public bool Static { get; set; }
        public bool Abstract { get; set; }
        public bool Sealed { get; set; }
        public bool Async { get; set; }
        public bool Override { get; set; }
        public bool Virtual { get; set; }
        public string Accessibility { get; set; }
    }
}