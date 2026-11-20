#nullable enable
using System;
using System.Linq;
using Godot;
namespace VMRuntime.GodotApi;

/// <summary>Editable declaration of a scalar or variadic JSON expression input.</summary>
[GlobalClass]
public partial class VmNodeInputDefinitionObject : RefCounted
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public Godot.Collections.Array<string> AcceptedTypes { get; set; } = new() { "any" };
    public bool Required { get; set; } = true;
    public bool Variadic { get; set; }
    public int MinItems { get; set; }
    public bool NullAsEmpty { get; set; }
    public string DefaultValueJson { get; set; } = "";
    public bool HasDefault => !string.IsNullOrWhiteSpace(DefaultValueJson);
    /// <summary>Prints this input declaration.</summary>
    public string ToJson() => ToCore().ToJson();
    internal VmNodeInputDefinition ToCore() => new()
    {
        Name = Name, Description = Description, AcceptedTypes = NodeDefinitionJson.Strings(AcceptedTypes),
        Required = Required, Variadic = Variadic, MinItems = MinItems, NullAsEmpty = NullAsEmpty,
        DefaultValue = NodeDefinitionJson.Default(DefaultValueJson)
    };
    internal static VmNodeInputDefinitionObject Wrap(VmNodeInputDefinition data) => new()
    {
        Name = data.Name, Description = data.Description, AcceptedTypes = new(data.AcceptedTypes),
        Required = data.Required, Variadic = data.Variadic, MinItems = data.MinItems, NullAsEmpty = data.NullAsEmpty,
        DefaultValueJson = data.DefaultValue?.GetRawText() ?? ""
    };
}
