using System.Collections.ObjectModel;
using VMRuntime.Diagnostics;

namespace VMRuntime;

/// <summary>One bytecode instruction; a/b/c are opcode-specific immediate operands.</summary>
public struct Instruction
{
    public OpcodeId Opcode;
    public int a;
    public int b;
    public int c;
}

/// <summary>Source and input binding for one compiled JSON operation.</summary>
public sealed class VmNodeInfo : VmJsonObject
{
    public int Id { get; }
    public VmSourceLocation Source { get; }
    /// <summary>Logical position in the merged document, independent of the original source file.</summary>
    public string Path { get; }
    public string Operation { get; }
    public string? ParentInput { get; }
    public int? ParentNodeId { get; }
    public IReadOnlyDictionary<string, VmValueSnapshot> Parameters { get; }
    public IReadOnlyList<string> InputNames { get; }
    public IReadOnlyList<string> OutputTypes { get; }
    public IReadOnlyList<IReadOnlyList<string>> InputTypes { get; }
    internal VmNodeInfo(int id, VmSourceLocation source, string operation, string? parentInput,
        Dictionary<string, VmValueSnapshot> parameters, IEnumerable<string> inputNames, int? parentNodeId = null, string? path = null,
        IReadOnlyList<string>? outputTypes = null, IReadOnlyList<IReadOnlyList<string>>? inputTypes = null)
    {
        (Id, Source, Operation, ParentInput) = (id, source, operation, parentInput);
        ParentNodeId = parentNodeId;
        Path = path ?? source.Path;
        Parameters = new ReadOnlyDictionary<string, VmValueSnapshot>(parameters);
        InputNames = Array.AsReadOnly(inputNames.ToArray());
        OutputTypes = Array.AsReadOnly((outputTypes ?? ["any"]).ToArray());
        InputTypes = Array.AsReadOnly((inputTypes ?? InputNames.Select(_ => (IReadOnlyList<string>)new[] { "any" }).ToArray())
            .Select(types => (IReadOnlyList<string>)Array.AsReadOnly(types.ToArray())).ToArray());
    }
}

/// <summary>Bytecode builder retained for legacy assemblers; compiled programs are frozen.</summary>
public sealed class Program
{
    private readonly List<Instruction> _instructions = new();
    private readonly List<int> _sources = new();
    private readonly List<DynValue> _constants = new();
    private readonly List<VmNodeInfo> _nodes = new();
    private readonly List<VmNodeCallBinding> _nodeBindings = new();
    private bool _frozen;
    /// <summary>Read-only view of the emitted instructions.</summary>
    public IReadOnlyList<Instruction> OpcodeList => _instructions.AsReadOnly();
    /// <summary>Number of emitted instructions.</summary>
    public int Count => _instructions.Count;
    /// <summary>Gets an instruction by zero-based index.</summary>
    public Instruction this[int index] => _instructions[index];
    internal IReadOnlyList<VmNodeInfo> Nodes => _nodes.AsReadOnly();
    internal IReadOnlyList<int> Sources => _sources.AsReadOnly();
    internal IReadOnlyList<DynValue> Constants => _constants.AsReadOnly();
    internal IReadOnlyList<VmNodeCallBinding> NodeBindings => _nodeBindings.AsReadOnly();

    /// <summary>Appends an instruction while the program is being assembled.</summary>
    public void Add(Instruction instruction) => Emit(instruction, -1);
    internal int Emit(Instruction instruction, int node)
    {
        if (_frozen) throw new InvalidOperationException("Compiled programs cannot be modified.");
        int index = Count;
        _instructions.Add(instruction);
        _sources.Add(node);
        return index;
    }
    internal void PatchTarget(int index, int target)
    {
        if (_frozen) throw new InvalidOperationException("Compiled programs cannot be modified.");
        var instruction = _instructions[index]; instruction.a = target; _instructions[index] = instruction;
    }
    internal int AddConstant(DynValue value) { _constants.Add(value); return _constants.Count - 1; }
    internal int AddNode(VmNodeInfo node) { _nodes.Add(node); return _nodes.Count - 1; }
    internal int AddNodeBinding(VmNodeCallBinding binding)
    { _nodeBindings.Add(binding); return _nodeBindings.Count - 1; }
    internal void Freeze() => _frozen = true;
    internal void Append(CompiledScript script)
    {
        if (_frozen) throw new InvalidOperationException("Compiled programs cannot be modified.");
        int instructionOffset = Count, constantOffset = _constants.Count, nodeOffset = _nodes.Count, bindingOffset = _nodeBindings.Count;
        _nodeBindings.AddRange(script.Bytecode.NodeBindings);
        foreach (var value in script.Bytecode.Constants) _constants.Add(value);
        foreach (var node in script.Nodes)
            _nodes.Add(new(node.Id + nodeOffset, node.Source, node.Operation, node.ParentInput,
                new(node.Parameters), node.InputNames, node.ParentNodeId is int parent ? parent + nodeOffset : null, node.Path, node.OutputTypes, node.InputTypes));
        for (int i = 0; i < script.InstructionCount; i++)
        {
            var op = script.Bytecode[i];
            if (op.Opcode is OpcodeId.EnterNode or OpcodeId.ExitNode or OpcodeId.AssertNodeInput) op.a += nodeOffset;
            if (op.Opcode is OpcodeId.PushConstant or OpcodeId.LoadVariable or OpcodeId.CallFunction) op.a += constantOffset;
            if (op.Opcode == OpcodeId.InvokeNode) op.a += bindingOffset;
            if (op.Opcode == OpcodeId.Jump || (op.Opcode is OpcodeId.JumpIfFalse or OpcodeId.JumpIfTrue && op.b == 1))
                op.a += instructionOffset;
            Emit(op, script.Bytecode.Sources[i] + nodeOffset);
        }
    }
}

/// <summary>Immutable reusable script, with original source and instruction-to-node mapping.</summary>
public sealed class CompiledScript : VmJsonObject
{
    public string Id { get; }
    public string SourceFile { get; }
    public string SourceHash { get; }
    public VmCompilationMetadata Metadata { get; }
    public string MetadataHash { get; }
    public string RootPath => Metadata.RootPath;
    public bool IsFragment { get; }
    public long RegistryVersion => NodeCatalog.RegistryVersion;
    public string RegistryHash => NodeCatalog.RegistryHash;
    [System.Text.Json.Serialization.JsonIgnore]
    public VmNodeCatalog NodeCatalog { get; }
    public int FormatVersion => 1;
    public string CompilerVersion => "1";
    public string OriginalJson { get; }
    public int InstructionCount => Bytecode.Count;
    public IReadOnlyList<VmNodeInfo> Nodes => Bytecode.Nodes;
    internal Program Bytecode { get; }
    internal CompiledScript(string id, string file, string hash, string json, Program program,
        VmCompilationMetadata? metadata = null, bool isFragment = false, VmNodeCatalog? nodeCatalog = null)
    {
        (Id, SourceFile, SourceHash, OriginalJson, Bytecode) = (id, file, hash, json, program);
        Metadata = (metadata ?? new() { ScriptId = id, SourceFile = file }).Snapshot();
        MetadataHash = Metadata.Fingerprint();
        IsFragment = isFragment;
        NodeCatalog = nodeCatalog ?? new VmNodeRegistry().GetCatalog();
        program.Freeze();
    }
    /// <summary>Gets the original source location of a zero-based instruction.</summary>
    public VmSourceLocation GetInstructionSource(int instructionIndex)
    {
        if ((uint)instructionIndex >= (uint)Bytecode.Count) throw new ArgumentOutOfRangeException(nameof(instructionIndex));
        int node = Bytecode.Sources[instructionIndex];
        return node >= 0 ? Bytecode.Nodes[node].Source : new(SourceFile);
    }
    /// <summary>Exports the exact catalog snapshot used to compile this handle, including after registry updates.</summary>
    public string GetNodeCatalogJson(bool indented = true) => NodeCatalog.ToJson(indented);
    /// <summary>Exports the bytecode, constants, node metadata and their source bindings as JSON.</summary>
    public string ExportProgramJson(bool indented = true)
        => System.Text.Json.JsonSerializer.Serialize(new
        {
            FormatVersion, CompilerVersion, Id, SourceFile, SourceHash, RootPath, IsFragment, Metadata, MetadataHash, RegistryVersion, RegistryHash, Nodes,
            NodeBindings = Bytecode.NodeBindings.Select((binding, index) => new
            { BindingIndex = index, binding.Definition.Operation, Inputs = binding.Inputs.Select(input => input.Key), Parameters = binding.Parameters.ToDictionary(p => p.Key, p => VmValueSnapshot.Capture(p.Value)) }),
            Constants = Bytecode.Constants.Select(VmValueSnapshot.Capture),
            Instructions = Bytecode.OpcodeList.Select((op, index) => new
            { InstructionIndex = index, Opcode = op.Opcode.ToString(), op.a, op.b, op.c, NodeId = Bytecode.Sources[index] })
        }, VmJson.Options(indented));
}
