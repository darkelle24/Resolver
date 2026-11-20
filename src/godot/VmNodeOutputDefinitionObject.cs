#nullable enable
using Godot;
namespace VMRuntime.GodotApi;

/// <summary>Editable output contract used by documentation and callback result validation.</summary>
[GlobalClass]
public partial class VmNodeOutputDefinitionObject : RefCounted
{
    public Godot.Collections.Array<string> Types { get; set; } = new() { "any" };
    public string Description { get; set; } = "Result of this expression.";
    public string Inference { get; set; } = "runtime";
    /// <summary>Prints the output contract.</summary>
    public string ToJson() => ToCore().ToJson();
    internal VmNodeOutputDefinition ToCore() => new()
    { Types = NodeDefinitionJson.Strings(Types), Description = Description, Inference = Inference };
    internal static VmNodeOutputDefinitionObject Wrap(VmNodeOutputDefinition data) => new()
    { Types = new(data.Types), Description = data.Description, Inference = data.Inference };
}
