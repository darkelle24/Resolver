#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
namespace VMRuntime.GodotApi;

/// <summary>Execution value, typed failure and optional explanation, usable from GDScript.</summary>
[GlobalClass]
public partial class VmExecutionResultObject : RefCounted
{
    internal VmExecutionResult? Data;
    internal Variant NativeValue;
    public bool Success => Data?.Success ?? false;
    public Variant Value => NativeValue;
    public VmExecutionErrorObject? Error
        => Data?.Error is { } error ? (VmExecutionErrorObject)VmErrorObject.Wrap(error) : null;
    public VmExplanationObject? Explanation
        => Data?.Explanation is { } explanation ? VmExplanationObject.Wrap(explanation) : null;
    /// <summary>Checks whether this invocation completed successfully.</summary>
    public bool IsSuccess() => Success;
    /// <summary>Gets the Godot-native result, or nil on failure.</summary>
    public Variant GetValue() => Value;
    /// <summary>Checks whether explanation was enabled for this invocation.</summary>
    public bool HasExplanation() => Data?.Explanation is not null;
    /// <summary>Prints the complete specialized error, or JSON null.</summary>
    public string GetErrorJson() => GetErrorJson(true);
    /// <summary>Serializes with an explicit indentation option.</summary>
    public string GetErrorJson(bool indented) => Data?.Error?.ToJson(indented) ?? "null";
    /// <summary>Prints the explanation tree, or JSON null.</summary>
    public string GetExplanationJson() => GetExplanationJson(true);
    /// <summary>Serializes with an explicit indentation option.</summary>
    public string GetExplanationJson(bool indented) => Data?.Explanation?.ToJson(indented) ?? "null";
    /// <summary>Prints the full invocation result.</summary>
    public string ToJson() => ToJson(true);
    /// <summary>Serializes with an explicit indentation option.</summary>
    public string ToJson(bool indented) => Data?.ToJson(indented) ?? "null";
    internal static VmExecutionResultObject Wrap(VmExecutionResult data)
        => new() { Data = data, NativeValue = data.Success ? DynValueConverter.ToVariant(data.Value) : default };
}
