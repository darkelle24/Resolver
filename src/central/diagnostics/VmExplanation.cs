namespace VMRuntime.Diagnostics;

/// <summary>Typed entry in an explanation input map; JSON serialization unwraps this adapter.</summary>
public abstract class VmTraceInput : VmJsonObject { }
/// <summary>A static parameter such as the name of a variable.</summary>
public sealed class VmTraceLiteral(VmValueSnapshot value) : VmTraceInput
{
    public VmValueSnapshot Value { get; } = value;
}
/// <summary>An evaluated child operation.</summary>
public sealed class VmTraceChild(VmTraceNode node) : VmTraceInput
{
    public VmTraceNode Node { get; } = node;
}
/// <summary>Repeated evaluations of the same input, in execution order.</summary>
public sealed class VmTraceOccurrences : VmTraceInput
{
    private readonly List<VmTraceNode> _nodes = new();
    public IReadOnlyList<VmTraceNode> Nodes => _nodes.AsReadOnly();
    internal void Add(VmTraceNode node) => _nodes.Add(node);
}
/// <summary>One actual evaluation of a JSON operation; unexecuted branches are absent.</summary>
public sealed class VmTraceNode : VmJsonObject
{
    private readonly Dictionary<string, VmTraceInput> _inputs = new(StringComparer.Ordinal);
    public long ExecutionId { get; }
    public int NodeId { get; }
    public string Path { get; }
    public VmSourceLocation Source { get; }
    public string Operation { get; }
    public IReadOnlyDictionary<string, VmTraceInput> Inputs => _inputs;
    public VmValueSnapshot? Output { get; internal set; }
    public VmExecutionError? Error { get; internal set; }
    public string Status { get; internal set; } = "running";
    public string? SelectedBranch { get; internal set; }
    internal VmTraceNode(long executionId, VmNodeInfo node)
    {
        (ExecutionId, NodeId, Path, Operation, Source) = (executionId, node.Id, node.Path, node.Operation, node.Source);
        foreach (var pair in node.Parameters) _inputs[pair.Key] = new VmTraceLiteral(pair.Value);
    }
    internal void Add(string name, VmTraceNode node)
    {
        if (!_inputs.TryGetValue(name, out var previous)) { _inputs[name] = new VmTraceChild(node); return; }
        if (previous is VmTraceOccurrences list) { list.Add(node); return; }
        var occurrences = new VmTraceOccurrences();
        if (previous is VmTraceChild child) occurrences.Add(child.Node);
        occurrences.Add(node);
        _inputs[name] = occurrences;
    }
}

/// <summary>Source-linked execution tree for one invocation.</summary>
public sealed class VmExplanation : VmJsonObject
{
    public int FormatVersion => 1;
    public string Script { get; }
    public string SourceFile { get; }
    public string SourceHash { get; }
    public string RootPath { get; }
    public string MetadataHash { get; }
    public bool IsFragment { get; }
    public long RegistryVersion { get; }
    public string RegistryHash { get; }
    public bool Success { get; internal set; }
    public bool Truncated { get; internal set; }
    public VmTraceNode? Tree { get; internal set; }
    internal VmExplanation(string script, string file, string hash, string rootPath = "", string metadataHash = "", bool isFragment = false,
        long registryVersion = 0, string registryHash = "")
        => (Script, SourceFile, SourceHash, RootPath, MetadataHash, IsFragment, RegistryVersion, RegistryHash)
            = (script, file, hash, rootPath, metadataHash, isFragment, registryVersion, registryHash);
}
