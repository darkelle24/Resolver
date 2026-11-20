namespace VMRuntime;

/// <summary>Source-aware bytecode builder available only while an advanced custom node is compiling.</summary>
public sealed class VmNodeCompilationContext
{
    private bool _open = true;
    private readonly Func<string, int> _compileInput;
    private readonly Func<OpcodeId, int, int, int, int> _emit;
    private readonly Func<DynValue, int> _constant;
    private readonly Func<int> _position;
    private readonly Action<int, int> _patch;
    private readonly Action<string, string?> _fail;
    public VmNodeDefinition Definition { get; }
    public IReadOnlyDictionary<string, DynValue> Parameters { get; }
    internal VmNodeCompilationContext(VmNodeDefinition definition, IReadOnlyDictionary<string, DynValue> parameters,
        Func<string, int> compileInput, Func<OpcodeId, int, int, int, int> emit, Func<DynValue, int> constant,
        Func<int> position, Action<int, int> patch, Action<string, string?> fail)
        => (Definition, Parameters, _compileInput, _emit, _constant, _position, _patch, _fail)
            = (definition, parameters, compileInput, emit, constant, position, patch, fail);
    private void Check() { if (!_open) throw new InvalidOperationException("This node compilation context is closed."); }
    /// <summary>Gets the next instruction index, suitable as an absolute jump target.</summary>
    public int Position { get { Check(); return _position(); } }
    /// <summary>Compiles a declared scalar input or all members of a variadic input in source order.</summary>
    public int CompileInput(string name) { Check(); return _compileInput(name); }
    /// <summary>Compiles every declared input in definition order.</summary>
    public int CompileAllInputs()
    { int count = 0; foreach (var input in Definition.Inputs) count += CompileInput(input.Name); return count; }
    /// <summary>Emits an instruction attributed to the current JSON node and returns its index.</summary>
    public int Emit(OpcodeId opcode, int a = 0, int b = 0, int c = 0)
    { Check(); return _emit(opcode, a, b, c); }
    /// <summary>Adds a detached constant and emits its load instruction.</summary>
    public void PushConstant(DynValue value)
    { Check(); _emit(OpcodeId.PushConstant, AddConstant(value), 0, 0); }
    /// <summary>Adds a detached constant and returns its pool index for immediate opcode operands.</summary>
    public int AddConstant(DynValue value)
    {
        Check();
        ValidateConstant(value, new HashSet<object>(ReferenceEqualityComparer.Instance), 0);
        return _constant(VM.CopyConstant(value));
    }
    private static void ValidateConstant(DynValue value, HashSet<object> path, int depth)
    {
        if (depth > 128) throw new ArgumentException("A compiled constant exceeds depth 128.");
        if (value.Type is not (ValueType.Array or ValueType.Dictionary)) return;
        object reference = value.RefValue!;
        if (!path.Add(reference)) throw new ArgumentException("Compiled collection constants cannot contain cycles.");
        if (value.IsArray)
            foreach (var item in value.AsArray()) ValidateConstant(item, path, depth + 1);
        else foreach (var pair in value.AsDictionary())
        { ValidateConstant(pair.Key, path, depth + 1); ValidateConstant(pair.Value, path, depth + 1); }
        path.Remove(reference);
    }
    /// <summary>Patches an immediate jump emitted by this context to an absolute instruction index.</summary>
    public void PatchJump(int instructionIndex, int target) { Check(); _patch(instructionIndex, target); }
    /// <summary>Stops compilation with a typed diagnostic at this node or one of its fields.</summary>
    public void Fail(string message, string? field = null) { Check(); _fail(message, field); }
    internal void Close() => _open = false;
}
