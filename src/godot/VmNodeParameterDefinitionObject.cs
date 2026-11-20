#nullable enable
using System;
using Godot;
namespace VMRuntime.GodotApi;

/// <summary>Editable compile-time literal parameter, with optional default and enum values.</summary>
[GlobalClass]
public partial class VmNodeParameterDefinitionObject : RefCounted
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public Godot.Collections.Array<string> AcceptedTypes { get; set; } = new() { "any" };
    public bool Required { get; set; } = true;
    public int MinLength { get; set; }
    public string DefaultValueJson { get; set; } = "";
    public bool HasDefault => !string.IsNullOrWhiteSpace(DefaultValueJson);
    public Godot.Collections.Array<string> EnumValues { get; set; } = new();
    /// <summary>Prints this parameter declaration.</summary>
    public string ToJson() => ToCore().ToJson();
    internal VmNodeParameterDefinition ToCore() => new()
    {
        Name = Name, Description = Description, AcceptedTypes = NodeDefinitionJson.Strings(AcceptedTypes),
        Required = Required, MinLength = MinLength, DefaultValue = NodeDefinitionJson.Default(DefaultValueJson),
        EnumValues = NodeDefinitionJson.Strings(EnumValues)
    };
    internal static VmNodeParameterDefinitionObject Wrap(VmNodeParameterDefinition data) => new()
    {
        Name = data.Name, Description = data.Description, AcceptedTypes = new(data.AcceptedTypes),
        Required = data.Required, MinLength = data.MinLength, DefaultValueJson = data.DefaultValue?.GetRawText() ?? "",
        EnumValues = new(data.EnumValues)
    };
}
