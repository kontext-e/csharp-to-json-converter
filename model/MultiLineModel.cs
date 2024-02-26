namespace csharp_to_json_converter.model
{
    public abstract class MultiLineModel
    {
        public int? FirstLineNumber { get; set; }
        public int? LastLineNumber { get; set; }
        
        // ReSharper disable once UnusedMember.Global
        public int? EffectiveLineCount => LastLineNumber - FirstLineNumber;
    }
}