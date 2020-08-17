using System.Collections.Generic;

namespace csharp_to_json_converter.model
{
    public class MethodModel
    {
        public string Name { get; set; }
        public string Fqn { get; set; }
        public bool Static { get; set; }
        public bool Abstract { get; set; }
        public bool Sealed { get; set; }
        public bool Async { get; set; }
        public bool Override { get; set; }
        public bool Virtual { get; set; }
        public string Accessibility { get; set; }
        public string ReturnType { get; set; }
        public int FirstLineNumber { get; set; }
        public int LastLineNumber { get; set; }

        // ReSharper disable once UnusedMember.Global
        public int EffectiveLineCount
        {
            get { return LastLineNumber - FirstLineNumber; }
        }

        public List<InvokesModel> Invocations { get; set; }
        public List<ParameterModel> Parameters { get; set; }

        public MethodModel()
        {
            Invocations = new List<InvokesModel>();
            Parameters = new List<ParameterModel>();
        }
    }
}