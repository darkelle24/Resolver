#nullable enable
using Godot;
namespace VMRuntime.GodotApi;

/// <summary>Typed outcome of a GDScript node registration.</summary>
[GlobalClass]
public partial class VmNodeRegistrationResultObject : RefCounted
{
    internal VmNodeRegistrationResult? Data;
    public bool Success => Data?.Success ?? false;
    public VmCompilationErrorObject? Error
        => Data?.Error is { } error ? (VmCompilationErrorObject)VmErrorObject.Wrap(error) : null;
    /// <summary>Prints the registration outcome.</summary>
    public string ToJson() => ToJson(true);
    /// <summary>Prints with explicit indentation.</summary>
    public string ToJson(bool indented) => Data?.ToJson(indented) ?? "null";
}
