#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using VMRuntime.Diagnostics;
namespace VMRuntime.GodotApi;

/// <summary>Runtime error shared by the result and the failing explanation node.</summary>
[GlobalClass]
public partial class VmExecutionErrorObject : VmErrorObject
{
    public VmJsonInstructionObject? JsonInstruction
        => Data is VmExecutionError error ? VmJsonInstructionObject.Wrap(error.JsonInstruction) : null;
    public VmBytecodeInstructionObject? BytecodeInstruction
        => Data is VmExecutionError error ? VmBytecodeInstructionObject.Wrap(error.BytecodeInstruction) : null;
}
