#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using VMRuntime.Diagnostics;
namespace VMRuntime.GodotApi;

/// <summary>One operation actually visited during execution.</summary>
[GlobalClass]
public partial class VmTraceNodeObject : RefCounted
{
    internal VmTraceNode? Data;
    public long ExecutionId => Data?.ExecutionId ?? 0;
    public int NodeId => Data?.NodeId ?? -1;
    public string Path => Data?.Path ?? "";
    public VmSourceLocationObject? Source => Data is null ? null : VmSourceLocationObject.Wrap(Data.Source);
    public string Operation => Data?.Operation ?? "";
    public string Status => Data?.Status ?? "";
    public string SelectedBranch => Data?.SelectedBranch ?? "";
    public VmValueSnapshotObject? Output
        => Data?.Output is { } output ? VmValueSnapshotObject.Wrap(output) : null;
    public VmExecutionErrorObject? Error
        => Data?.Error is { } error ? (VmExecutionErrorObject)VmErrorObject.Wrap(error) : null;
    public Godot.Collections.Dictionary<string, VmTraceInputObject> Inputs
    {
        get
        {
            var result = new Godot.Collections.Dictionary<string, VmTraceInputObject>();
            if (Data is not null)
                foreach (var pair in Data.Inputs) result[pair.Key] = VmTraceInputObject.Wrap(pair.Value);
            return result;
        }
    }
    /// <summary>Prints this subtree with nested input operations.</summary>
    public string ToJson() => ToJson(true);
    /// <summary>Serializes with an explicit indentation option.</summary>
    public string ToJson(bool indented) => Data?.ToJson(indented) ?? "null";
    internal static VmTraceNodeObject Wrap(VmTraceNode data) => new() { Data = data };
}
