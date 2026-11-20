#nullable enable
using Godot;
namespace VMRuntime.GodotApi;

/// <summary>Typed view of the exact registry snapshot exported to the future web editor.</summary>
[GlobalClass]
public partial class VmNodeCatalogObject : RefCounted
{
    internal VmNodeCatalog? Data;
    public int FormatVersion => Data?.FormatVersion ?? 1;
    public long RegistryVersion => Data?.RegistryVersion ?? 0;
    public string RegistryHash => Data?.RegistryHash ?? "";
    public Godot.Collections.Array<string> ValueTypes => Data is null ? new() : new(Data.ValueTypes);
    public Godot.Collections.Array<VmNodeDefinitionObject> Nodes
    {
        get
        {
            var result = new Godot.Collections.Array<VmNodeDefinitionObject>();
            if (Data is not null) foreach (var node in Data.Nodes) result.Add(VmNodeDefinitionObject.Wrap(node));
            return result;
        }
    }
    public Godot.Collections.Array<VmLiteralTypeDefinitionObject> LiteralTypes
    {
        get
        {
            var result = new Godot.Collections.Array<VmLiteralTypeDefinitionObject>();
            if (Data is not null) foreach (var literal in Data.LiteralTypes) result.Add(new() { Data = literal });
            return result;
        }
    }
    /// <summary>Prints this versioned registry snapshot as JSON.</summary>
    public string ToJson() => ToJson(true);
    /// <summary>Prints with explicit indentation.</summary>
    public string ToJson(bool indented) => Data?.ToJson(indented) ?? "null";
    /// <summary>Prints common syntax conventions.</summary>
    public string GetSyntaxJson() => Data?.Syntax.ToJson() ?? "null";
}
