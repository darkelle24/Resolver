#nullable enable
using Godot;
namespace VMRuntime.GodotApi;

/// <summary>Read-only description of a tagged vector/color literal.</summary>
[GlobalClass]
public partial class VmLiteralTypeDefinitionObject : RefCounted
{
    internal VmLiteralTypeDefinition? Data;
    public string Tag => Data?.Tag ?? "";
    public string Description => Data?.Description ?? "";
    public Godot.Collections.Array<VmNodeParameterDefinitionObject> Components
    {
        get
        {
            var result = new Godot.Collections.Array<VmNodeParameterDefinitionObject>();
            if (Data is not null) foreach (var component in Data.Components) result.Add(VmNodeParameterDefinitionObject.Wrap(component));
            return result;
        }
    }
    /// <summary>Prints this literal declaration.</summary>
    public string ToJson() => Data?.ToJson() ?? "null";
}
