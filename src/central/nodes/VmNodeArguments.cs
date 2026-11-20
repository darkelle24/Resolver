using System.Collections.ObjectModel;

namespace VMRuntime;

/// <summary>Typed implementation for an eagerly evaluated project node.</summary>
public interface IVmJsonNode
{
    /// <summary>Gets this node's public syntax and documentation contract.</summary>
    VmNodeDefinition Definition { get; }
    /// <summary>Calculates the output from already evaluated inputs and literal parameters.</summary>
    DynValue Execute(VmNodeArguments arguments);
}

/// <summary>Advanced C# node implementation controlling bytecode emission and branch evaluation.</summary>
public interface IVmJsonNodeCompiler
{
    VmNodeDefinition Definition { get; }
    /// <summary>Emits one output value using the supplied source-aware compilation context.</summary>
    void Compile(VmNodeCompilationContext context);
}

/// <summary>Inputs and detached static parameters passed to one custom-node invocation.</summary>
public sealed class VmNodeArguments
{
    public IReadOnlyDictionary<string, DynValue> Inputs { get; }
    public IReadOnlyDictionary<string, DynValue> Parameters { get; }
    private readonly IReadOnlyDictionary<string, IReadOnlyList<DynValue>> _groups;
    private readonly HashSet<string> _variadic;
    internal VmNodeArguments(VmNodeCallBinding binding, DynValue[] inputs)
    {
        _variadic = binding.Definition.Inputs.Where(input => input.Variadic).Select(input => input.Name).ToHashSet(StringComparer.Ordinal);
        var values = new Dictionary<string, DynValue>(StringComparer.Ordinal);
        var groups = new Dictionary<string, IReadOnlyList<DynValue>>(StringComparer.Ordinal);
        foreach (var input in binding.Definition.Inputs)
        {
            var group = new List<DynValue>();
            for (int i = 0; i < inputs.Length; i++)
                if (binding.Inputs[i].Definition.Name == input.Name)
                { values.Add(binding.Inputs[i].Key, inputs[i]); group.Add(inputs[i]); }
            groups.Add(input.Name, group.AsReadOnly());
        }
        Inputs = new ReadOnlyDictionary<string, DynValue>(values);
        _groups = new ReadOnlyDictionary<string, IReadOnlyList<DynValue>>(groups);
        Parameters = new ReadOnlyDictionary<string, DynValue>(binding.Parameters.ToDictionary(p => p.Key, p => VM.CopyConstant(p.Value)));
    }
    /// <summary>Reads a scalar input by its declared name.</summary>
    public DynValue GetInput(string name)
    {
        var values = _groups[name];
        if (_variadic.Contains(name) || values.Count != 1) throw new InvalidOperationException($"Input '{name}' is variadic; use GetInputs.");
        return values[0];
    }
    /// <summary>Reads all evaluations of a variadic input in source order.</summary>
    public IReadOnlyList<DynValue> GetInputs(string name) => _groups[name];
    /// <summary>Reads a declared compile-time literal parameter.</summary>
    public DynValue GetParameter(string name) => Parameters[name];
}

internal sealed record VmBoundNodeInput(string Key, VmNodeInputDefinition Definition);
internal sealed record VmNodeCallBinding(VmNodeDefinition Definition, Func<VmNodeArguments, DynValue> Callback,
    IReadOnlyList<VmBoundNodeInput> Inputs, IReadOnlyDictionary<string, DynValue> Parameters);
