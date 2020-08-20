namespace csharp_to_json_converter.model
{
    public abstract class AccessModifierModel : MultiLineModel
    {
        public bool Static { get; set; }
        public bool Sealed { get; set; }
        public string Accessibility { get; set; }
        public bool Async { get; set; }
        public bool Override { get; set; }
        public bool Virtual { get; set; }
        public bool Extern { get; set; }
    }
}