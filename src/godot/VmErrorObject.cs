#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using VMRuntime.Diagnostics;
namespace VMRuntime.GodotApi;

/// <summary>Common Godot-visible root for all VM error classes.</summary>
[GlobalClass]
public partial class VmErrorObject : RefCounted
{
    internal VmError? Data;
    public string Kind => Data?.Kind ?? "";
    public string Code => Data?.Code ?? "";
    public string Message => Data?.Message ?? "";
    public VmSourceLocationObject Source => VmSourceLocationObject.Wrap(Data?.Source ?? new("<unknown>"));
    /// <summary>Prints the complete specialized diagnostic as JSON.</summary>
    public string ToJson() => ToJson(true);
    /// <summary>Serializes with an explicit indentation option.</summary>
    public string ToJson(bool indented) => Data?.ToJson(indented) ?? "null";
    internal static VmErrorObject Wrap(VmError data)
    {
        VmErrorObject result = data switch
        {
            VmTypeMismatchError => new VmTypeMismatchErrorObject(),
            VmVariableNotFoundError => new VmVariableNotFoundErrorObject(),
            VmDivisionByZeroError => new VmDivisionByZeroErrorObject(),
            VmHostCallError => new VmHostCallErrorObject(),
            VmExecutionError => new VmExecutionErrorObject(),
            VmMissingFieldError => new VmMissingFieldErrorObject(),
            VmJsonSyntaxError => new VmJsonSyntaxErrorObject(),
            _ => new VmCompilationErrorObject()
        };
        result.Data = data;
        return result;
    }
}
