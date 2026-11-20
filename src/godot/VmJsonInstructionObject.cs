#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using VMRuntime.Diagnostics;
namespace VMRuntime.GodotApi;

/// <summary>Failed JSON operation with flat typed input snapshots.</summary>
[GlobalClass]
public partial class VmJsonInstructionObject : RefCounted
{
    internal VmJsonInstructionContext? Data;
    public string Operation => Data?.Operation ?? "";
    public string Path => Data?.Path ?? "";
    public Godot.Collections.Dictionary<string, VmValueSnapshotObject> Inputs
        => Data is null ? new() : VmValueSnapshotObject.Map(Data.Inputs);
    /// <summary>Prints the local JSON operation context.</summary>
    public string ToJson() => ToJson(true);
    /// <summary>Serializes with an explicit indentation option.</summary>
    public string ToJson(bool indented) => Data?.ToJson(indented) ?? "null";
    internal static VmJsonInstructionObject Wrap(VmJsonInstructionContext data) => new() { Data = data };
}
