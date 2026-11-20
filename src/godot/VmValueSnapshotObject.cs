#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using VMRuntime.Diagnostics;
namespace VMRuntime.GodotApi;

/// <summary>Detached input or output value captured at execution time.</summary>
[GlobalClass]
public partial class VmValueSnapshotObject : RefCounted
{
    internal VmValueSnapshot Data = VmValueSnapshot.Unavailable("Not initialized.");
    public string Type => Data.Type;
    public Variant Value => Json.ParseString(Data.Value.GetRawText());
    public bool Truncated => Data.Truncated;
    /// <summary>Prints the typed snapshot as JSON.</summary>
    public string ToJson() => ToJson(true);
    /// <summary>Serializes with an explicit indentation option.</summary>
    public string ToJson(bool indented) => Data.ToJson(indented);
    internal static VmValueSnapshotObject Wrap(VmValueSnapshot data) => new() { Data = data };
    internal static Godot.Collections.Dictionary<string, VmValueSnapshotObject> Map(IReadOnlyDictionary<string, VmValueSnapshot> data)
    {
        var result = new Godot.Collections.Dictionary<string, VmValueSnapshotObject>();
        foreach (var pair in data) result[pair.Key] = Wrap(pair.Value);
        return result;
    }
}
