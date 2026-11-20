#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using VMRuntime.Diagnostics;
namespace VMRuntime.GodotApi;

/// <summary>Godot-visible specialization for VmJsonSyntaxError.</summary>
[GlobalClass]
public partial class VmJsonSyntaxErrorObject : VmCompilationErrorObject
{

}
