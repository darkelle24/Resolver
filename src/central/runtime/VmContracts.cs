using VMRuntime.Diagnostics;

namespace VMRuntime;

/// <summary>Options for one invocation; enabling explanation never recompiles the script.</summary>
public sealed class VmExecutionOptions
{
    /// <summary>Records the actual evaluation tree when true.</summary>
    public bool Explain { get; init; }
    /// <summary>Maximum recorded operation evaluations; execution continues when reached.</summary>
    public int MaxTraceSteps { get; init; } = 10_000;
    /// <summary>Maximum executed bytecode instructions, including control and bookkeeping instructions.</summary>
    public int MaxInstructions { get; init; } = 1_000_000;
    /// <summary>Cumulative script allocation units (collection slots and concatenated characters).</summary>
    public int MaxAllocatedValues { get; init; } = 1_000_000;
    public int MaxCollectionItems { get; init; } = 16_384;
    public int MaxStringLength { get; init; } = 65_536;
    internal void Validate()
    {
        if (MaxTraceSteps < 0) throw new ArgumentOutOfRangeException(nameof(MaxTraceSteps));
        if (MaxInstructions <= 0) throw new ArgumentOutOfRangeException(nameof(MaxInstructions));
        if (MaxAllocatedValues <= 0) throw new ArgumentOutOfRangeException(nameof(MaxAllocatedValues));
        if (MaxCollectionItems <= 0) throw new ArgumentOutOfRangeException(nameof(MaxCollectionItems));
        if (MaxStringLength <= 0) throw new ArgumentOutOfRangeException(nameof(MaxStringLength));
    }
}

/// <summary>Compilation outcome; errors are available without exceptions crossing the public API.</summary>
public sealed class VmCompilationResult : VmJsonObject
{
    public bool Success => Script is not null;
    public CompiledScript? Script { get; }
    public VmCompilationError? Error { get; }
    internal VmCompilationResult(CompiledScript? script, VmCompilationError? error)
        => (Script, Error) = (script, error);
    /// <summary>Creates a failed compilation result for a source-loader diagnostic.</summary>
    public static VmCompilationResult Failed(VmCompilationError error)
        => new(null, error ?? throw new ArgumentNullException(nameof(error)));
}

/// <summary>One invocation outcome; Value is meaningful when Success is true.</summary>
public sealed class VmExecutionResult : VmJsonObject
{
    public bool Success => Error is null;
    [System.Text.Json.Serialization.JsonIgnore]
    public DynValue Value { get; }
    [System.Text.Json.Serialization.JsonPropertyName("value")]
    public VmValueSnapshot? ValueSnapshot { get; }
    public VmExecutionError? Error { get; }
    public VmExplanation? Explanation { get; }
    internal VmExecutionResult(DynValue value, VmExecutionError? error, VmExplanation? explanation)
    {
        (Value, Error, Explanation) = (value, error, explanation);
        ValueSnapshot = error is null ? VmValueSnapshot.Capture(value) : null;
    }
    /// <summary>Creates a failed invocation result for a boundary/conversion diagnostic.</summary>
    public static VmExecutionResult Failed(VmExecutionError error, VmExplanation? explanation = null)
    {
        ArgumentNullException.ThrowIfNull(error);
        if (explanation is not null)
        {
            explanation.Success = false;
            if (explanation.Tree is { } root)
            {
                root.Error = error;
                root.Output = null;
                root.Status = "failed";
            }
        }
        return new(DynValue.Null, error, explanation);
    }
}

/// <summary>Engine-independent access to host objects. Implementations define allowed operations.</summary>
public interface IVmHostObjects
{
    /// <summary>Reads a named property from a host object.</summary>
    DynValue Get(object target, string property);
    /// <summary>Writes a named property on a host object.</summary>
    void Set(object target, string property, DynValue value);
    /// <summary>Calls an allowed host method with arguments in source order.</summary>
    DynValue Call(object target, string method, IReadOnlyList<DynValue> arguments);
    /// <summary>Checks whether the host reference is still valid.</summary>
    bool IsValid(object target);
    /// <summary>Releases an object if the host permits it.</summary>
    void Free(object target);
    /// <summary>Lists the properties available on this object.</summary>
    IReadOnlyList<string> GetProperties(object target);
    /// <summary>Lists the methods permitted on this object.</summary>
    IReadOnlyList<string> GetMethods(object target);
}

internal sealed class UnavailableHostObjects : IVmHostObjects
{
    private static Exception Missing() => new NotSupportedException("Supply an IVmHostObjects adapter to access host objects.");
    public DynValue Get(object target, string property) => throw Missing();
    public void Set(object target, string property, DynValue value) => throw Missing();
    public DynValue Call(object target, string method, IReadOnlyList<DynValue> arguments) => throw Missing();
    public bool IsValid(object target) => target is not null;
    public void Free(object target) => throw Missing();
    public IReadOnlyList<string> GetProperties(object target) => throw Missing();
    public IReadOnlyList<string> GetMethods(object target) => throw Missing();
}
