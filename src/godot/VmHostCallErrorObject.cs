#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using VMRuntime.Diagnostics;
namespace VMRuntime.GodotApi;

/// <summary>Godot-visible specialization for VmHostCallError.</summary>
[GlobalClass]
public partial class VmHostCallErrorObject : VmExecutionErrorObject
{
    public string Method => (Data as VmHostCallError)?.Method ?? "";
}
