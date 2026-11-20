#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
namespace VMRuntime.GodotApi;

/// <summary>Common Godot-visible root for source loading and compilation errors.</summary>
[GlobalClass]
public partial class VmCompilationErrorObject : VmErrorObject { }
