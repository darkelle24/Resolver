#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
namespace VMRuntime.GodotApi;

/// <summary>Typed per-execution options usable from GDScript.</summary>
[GlobalClass]
public partial class VmExecutionOptionsObject : RefCounted
{
    public bool Explain { get; set; }
    public int MaxTraceSteps { get; set; } = 10_000;
    public int MaxInstructions { get; set; } = 1_000_000;
    public int MaxAllocatedValues { get; set; } = 1_000_000;
    public int MaxCollectionItems { get; set; } = 16_384;
    public int MaxStringLength { get; set; } = 65_536;
    internal VmExecutionOptions ToCore() => new()
    { Explain = Explain, MaxTraceSteps = MaxTraceSteps, MaxInstructions = MaxInstructions,
        MaxAllocatedValues = MaxAllocatedValues, MaxCollectionItems = MaxCollectionItems, MaxStringLength = MaxStringLength };
}
