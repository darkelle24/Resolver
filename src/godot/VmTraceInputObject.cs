#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using VMRuntime.Diagnostics;
namespace VMRuntime.GodotApi;

/// <summary>A literal parameter, evaluated child, or ordered repeated evaluations.</summary>
[GlobalClass]
public partial class VmTraceInputObject : RefCounted
{
    internal VmTraceInput? Data;
    public string Kind => Data switch
    { VmTraceLiteral => "literal", VmTraceChild => "node", VmTraceOccurrences => "occurrences", _ => "unavailable" };
    public VmValueSnapshotObject? Literal
        => Data is VmTraceLiteral literal ? VmValueSnapshotObject.Wrap(literal.Value) : null;
    public VmTraceNodeObject? Node
        => Data is VmTraceChild child ? VmTraceNodeObject.Wrap(child.Node) : null;
    public Godot.Collections.Array<VmTraceNodeObject> Occurrences
    {
        get
        {
            var result = new Godot.Collections.Array<VmTraceNodeObject>();
            if (Data is VmTraceOccurrences list)
                foreach (var node in list.Nodes) result.Add(VmTraceNodeObject.Wrap(node));
            return result;
        }
    }
    /// <summary>Prints this input's typed representation.</summary>
    public string ToJson() => ToJson(true);
    /// <summary>Serializes with an explicit indentation option.</summary>
    public string ToJson(bool indented) => Data?.ToJson(indented) ?? "null";
    internal static VmTraceInputObject Wrap(VmTraceInput data) => new() { Data = data };
}
