#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using VMRuntime.Diagnostics;
namespace VMRuntime.GodotApi;

/// <summary>Godot-visible specialization for VmMissingFieldError.</summary>
[GlobalClass]
public partial class VmMissingFieldErrorObject : VmCompilationErrorObject
{
    public string Field => (Data as VmMissingFieldError)?.Field ?? "";
}
