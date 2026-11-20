#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
namespace VMRuntime.GodotApi;

/// <summary>Typed result of loading and compiling a script.</summary>
[GlobalClass]
public partial class VmCompilationResultObject : RefCounted
{
    internal VmCompilationResult? Data;
    public bool Success => Data?.Success ?? false;
    public VmCompiledScriptObject? Script
        => Data?.Script is CompiledScript script ? VmCompiledScriptObject.Wrap(script) : null;
    public VmCompilationErrorObject? Error
        => Data?.Error is { } error ? (VmCompilationErrorObject)VmErrorObject.Wrap(error) : null;
    /// <summary>Prints the compilation result and any source error.</summary>
    public string ToJson() => ToJson(true);
    /// <summary>Serializes with an explicit indentation option.</summary>
    public string ToJson(bool indented) => Data?.ToJson(indented) ?? "null";
    internal static VmCompilationResultObject Wrap(VmCompilationResult data) => new() { Data = data };
}
