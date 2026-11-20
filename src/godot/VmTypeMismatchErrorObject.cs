#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using VMRuntime.Diagnostics;
namespace VMRuntime.GodotApi;

/// <summary>Godot-visible specialization for VmTypeMismatchError.</summary>
[GlobalClass]
public partial class VmTypeMismatchErrorObject : VmExecutionErrorObject
{
    public string ExpectedType => (Data as VmTypeMismatchError)?.ExpectedType ?? "";
    public string ActualType => (Data as VmTypeMismatchError)?.ActualType ?? "";
}
