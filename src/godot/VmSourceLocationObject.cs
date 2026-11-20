#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using VMRuntime.Diagnostics;
namespace VMRuntime.GodotApi;

/// <summary>Source location accessible from C# and GDScript.</summary>
[GlobalClass]
public partial class VmSourceLocationObject : RefCounted
{
    internal VmSourceLocation Data = new("<unknown>");
    public string File => Data.File;
    public string Path => Data.Path;
    public long Line => Data.Line;
    public long ByteColumn => Data.ByteColumn;
    public bool HasPosition => Data.HasPosition;
    /// <summary>Prints this location as JSON.</summary>
    public string ToJson() => ToJson(true);
    /// <summary>Serializes with an explicit indentation option.</summary>
    public string ToJson(bool indented) => Data.ToJson(indented);
    internal static VmSourceLocationObject Wrap(VmSourceLocation data) => new() { Data = data };
}
