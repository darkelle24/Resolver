using System.Collections.Generic;

namespace VM.RuleNodeSystem
{
    public class RuleNodeMeta
    {
        public string TypeName { get; set; } = "";
        public string Description { get; set; } = "";
        public string Example { get; set; } = "";
        public List<FieldMeta> StaticFields { get; } = new();
        public List<FieldMeta> InputFields { get; } = new();
        public List<FieldMeta> VariadicInputs { get; } = new();
    }

    public class FieldMeta
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public bool Required { get; set; }
        public string[] AcceptedTypes { get; set; } = Array.Empty<string>();
        public string[] AllowedValues { get; set; } = Array.Empty<string>();
        public object? DefaultValue { get; set; }
        public bool SkipAutoValidation { get; set; }
    }
}