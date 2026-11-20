#nullable enable
using System;
using System.Collections.Generic;
using Godot;
namespace VMRuntime.GodotApi;

/// <summary>Typed context for compiling a fragment from merged game data.</summary>
[GlobalClass]
public partial class VmCompilationMetadataObject : RefCounted
{
    /// <summary>Optional identifier, for example items/sword:onAttack; empty uses file plus root path.</summary>
    public string ScriptId { get; set; } = "";
    /// <summary>Fallback source identifier. Fragment compilation performs no file I/O.</summary>
    public string SourceFile { get; set; } = "<merged>";
    /// <summary>Fragment's JSON Pointer in the merged item/document, for example /onAttack.</summary>
    public string RootPath { get; set; } = "";
    /// <summary>Original source positions keyed by pointers relative to the fragment.</summary>
    public Godot.Collections.Dictionary<string, VmSourceOriginObject> SourceMap { get; set; } = new();
    /// <summary>Application labels such as item_id or mod_id.</summary>
    public Godot.Collections.Dictionary<string, string> Tags { get; set; } = new();

    /// <summary>Serializes the supplied metadata.</summary>
    public string ToJson() => ToJson(true);
    /// <summary>Serializes with explicit indentation.</summary>
    public string ToJson(bool indented) => ToCore().ToJson(indented);
    internal VmCompilationMetadata ToCore()
    {
        ArgumentNullException.ThrowIfNull(SourceMap);
        ArgumentNullException.ThrowIfNull(Tags);
        var origins = new Dictionary<string, VMRuntime.Diagnostics.VmSourceLocation>(StringComparer.Ordinal);
        foreach (var pair in SourceMap)
        {
            if (pair.Value is null) throw new ArgumentException($"SourceMap entry '{pair.Key}' must be a VmSourceOriginObject.");
            origins.Add(pair.Key, pair.Value.ToCore());
        }
        var tags = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in Tags) tags.Add(pair.Key, pair.Value);
        return new()
        {
            ScriptId = string.IsNullOrEmpty(ScriptId) ? null : ScriptId,
            SourceFile = SourceFile, RootPath = RootPath, SourceMap = origins, Tags = tags
        };
    }
    internal static VmCompilationMetadataObject Wrap(VmCompilationMetadata data)
    {
        var result = new VmCompilationMetadataObject
        { ScriptId = data.ScriptId ?? "", SourceFile = data.SourceFile, RootPath = data.RootPath };
        foreach (var pair in data.SourceMap) result.SourceMap[pair.Key] = VmSourceOriginObject.Wrap(pair.Value);
        foreach (var pair in data.Tags) result.Tags[pair.Key] = pair.Value;
        return result;
    }
}
