#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using VMRuntime.Diagnostics;
namespace VMRuntime.GodotApi;

/// <summary>Godot-visible specialization for VmVariableNotFoundError.</summary>
[GlobalClass]
public partial class VmVariableNotFoundErrorObject : VmExecutionErrorObject
{
    public string VariableName => (Data as VmVariableNotFoundError)?.VariableName ?? "";
}
