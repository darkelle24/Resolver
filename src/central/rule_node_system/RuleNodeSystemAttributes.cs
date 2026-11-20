namespace VM.RuleNodeSystem
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class RuleNodeTypeAttribute : Attribute
    {
        public string TypeName { get; }
        public string Description { get; set; } = "";
        public string Example { get; set; } = "";
        public RuleNodeTypeAttribute(string typeName) => TypeName = typeName;
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class StaticFieldAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; set; } = "";
        public bool Required { get; set; } = false;
        public object? DefaultValue { get; set; } = null;
        public string[] AllowedValues { get; set; } = Array.Empty<string>();
        public bool SkipAutoValidation { get; set; } = false;
        public StaticFieldAttribute(string name) => Name = name;
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class InputFieldAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; set; } = "";
        public bool Required { get; set; } = true;
        public string[] AcceptedTypes { get; set; } = Array.Empty<string>();
        public bool SkipAutoValidation { get; set; } = false;
        public InputFieldAttribute(string name) => Name = name;
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class VariadicInputAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; set; } = "";
        public bool Required { get; set; } = false;
        public string[] AcceptedTypes { get; set; } = Array.Empty<string>();
        public bool SkipAutoValidation { get; set; } = false;
        public VariadicInputAttribute(string name) => Name = name;
    }
}