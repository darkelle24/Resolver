#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using VMRuntime.Diagnostics;
namespace VMRuntime.GodotApi;

/// <summary>Failed bytecode instruction, immediate operands and runtime inputs.</summary>
[GlobalClass]
public partial class VmBytecodeInstructionObject : RefCounted
{
    internal VmBytecodeInstructionContext? Data;
    public int InstructionIndex => Data?.InstructionIndex ?? -1;
    public string Opcode => Data?.Opcode ?? "";
    public Godot.Collections.Dictionary<string, VmValueSnapshotObject> Inputs
        => Data is null ? new() : VmValueSnapshotObject.Map(Data.Inputs);
    public Godot.Collections.Dictionary<string, int> Operands
    {
        get
        {
            var result = new Godot.Collections.Dictionary<string, int>();
            if (Data is not null) foreach (var pair in Data.Operands) result[pair.Key] = pair.Value;
            return result;
        }
    }
    /// <summary>Prints the bytecode failure context.</summary>
    public string ToJson() => ToJson(true);
    /// <summary>Serializes with an explicit indentation option.</summary>
    public string ToJson(bool indented) => Data?.ToJson(indented) ?? "null";
    internal static VmBytecodeInstructionObject Wrap(VmBytecodeInstructionContext data) => new() { Data = data };
}
