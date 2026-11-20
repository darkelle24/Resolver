using VMRuntime.Diagnostics;
namespace VMRuntime;

/// <summary>Registration outcome used at language boundaries that should not propagate C# argument exceptions.</summary>
public sealed class VmNodeRegistrationResult : VmJsonObject
{
    public bool Success => Error is null;
    public VmCompilationError? Error { get; }
    /// <summary>Creates a successful registration, or a failure with its configuration diagnostic.</summary>
    public VmNodeRegistrationResult(VmCompilationError? error = null) => Error = error;
}
