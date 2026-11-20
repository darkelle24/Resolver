using VMRuntime.Diagnostics;
using VMRuntime.Errors;

namespace VMRuntime;

/// <summary>One isolated execution state. Use VmEngine to reuse compiled scripts across invocations.</summary>
public partial class VM
{
    /// <summary>Program associated with this VM instance.</summary>
    public readonly Program Program;
    /// <summary>Operand stack, exposed for low-level opcode implementations.</summary>
    public readonly Stack<DynValue> Stack = new();
    /// <summary>Adapter responsible for host object access.</summary>
    public IVmHostObjects Host { get; }
    private readonly IReadOnlyDictionary<string, DynValue> _variables;
    private readonly IReadOnlyDictionary<string, Func<IReadOnlyList<DynValue>, DynValue>> _functions;
    private readonly CompiledScript? _script;
    private readonly System.Collections.Generic.Stack<Frame> _frames = new();
    private readonly Dictionary<string, DynValue> _instructionInputs = new(StringComparer.Ordinal);
    private Dictionary<string, VmValueSnapshot>? _instructionSnapshots;
    private VmExplanation? _explanation;
    private VmExecutionOptions _options = new();
    private long _executionId;
    private int _recordedNodes;
    private long _allocatedValues;

    /// <summary>Creates a low-level VM; supply a host adapter for object opcodes.</summary>
    public VM(IVmHostObjects? host = null)
        : this(new Program(), null, null, host, null, null) { }

    internal VM(Program program, CompiledScript? script, IReadOnlyDictionary<string, DynValue>? variables,
        IVmHostObjects? host, IReadOnlyDictionary<string, Func<IReadOnlyList<DynValue>, DynValue>>? functions,
        VmExecutionOptions? options)
    {
        Program = program;
        _script = script;
        _variables = variables ?? new Dictionary<string, DynValue>();
        Host = host ?? new UnavailableHostObjects();
        _functions = functions ?? new Dictionary<string, Func<IReadOnlyList<DynValue>, DynValue>>();
        _options = options ?? new();
    }

    /// <summary>Runs low-level assembled bytecode; failures are thrown for legacy callers.</summary>
    public void Run()
    {
        _allocatedValues = 0;
        _options.Validate();
        int ip = 0, count = 0;
        while (ip != Program.Count)
        {
            if ((uint)ip >= (uint)Program.Count) throw new InvalidOperationException("Invalid instruction address.");
            if (count++ >= _options.MaxInstructions) throw new InvalidOperationException("Instruction limit reached.");
            ip = ExecuteInstruction(Program[ip].Opcode, ip);
        }
    }

    /// <summary>Runs this program with a fresh stack and returns a structured outcome.</summary>
    public VmExecutionResult Execute()
    {
        _options.Validate();
        Stack.Clear(); _frames.Clear(); _executionId = 0; _recordedNodes = 0; _allocatedValues = 0;
        _explanation = _options.Explain ? new(_script?.Id ?? "<assembled>",
            _script?.SourceFile ?? "<assembled>", _script?.SourceHash ?? "", _script?.RootPath ?? "",
            _script?.MetadataHash ?? "", _script?.IsFragment ?? false, _script?.RegistryVersion ?? 0, _script?.RegistryHash ?? "") : null;
        int ip = 0, executed = 0;
        Instruction instruction = default;
        try
        {
            while (ip != Program.Count)
            {
                if ((uint)ip >= (uint)Program.Count) throw new VmRuntimeFault("INVALID_JUMP", $"Invalid instruction address {ip}.");
                instruction = Program[ip];
                CaptureInstructionInputs(instruction);
                if (executed++ >= _options.MaxInstructions)
                    throw new VmRuntimeFault("EXECUTION_LIMIT", "The execution instruction limit was reached.");
                ip = ExecuteInstruction(instruction.Opcode, ip);
            }
            if (_explanation is not null) _explanation.Success = true;
            return new(Stack.IsEmpty() ? DynValue.Null : Stack.Peek(), null, _explanation);
        }
        catch (Exception error) when (error is not OutOfMemoryException)
        {
            if (instruction.Opcode == OpcodeId.EnterNode && (uint)instruction.a < (uint)Program.Nodes.Count &&
                (!_frames.TryPeek(out var current) || current.Info.Id != instruction.a)) EnterNode(instruction.a);
            var diagnostic = BuildError(error, ip, instruction);
            PreserveFailurePath();
            foreach (var frame in _frames)
                if (frame.Trace is not null) frame.Trace.Status = "aborted";
            if (_frames.TryPeek(out var active) && active.Trace is not null)
            {
                active.Trace.Error = diagnostic;
                active.Trace.Status = "failed";
            }
            return new(DynValue.Null, diagnostic, _explanation);
        }
        finally { Stack.Clear(); _frames.Clear(); _instructionInputs.Clear(); _instructionSnapshots = null; }
    }

    private void PreserveFailurePath()
    {
        if (_explanation is null) return;
        Frame? parent = null;
        foreach (var frame in _frames.Reverse())
        {
            if (frame.Trace is null)
            {
                frame.Trace = new(++_executionId, frame.Info);
                if (parent?.Trace is not null) parent.Trace.Add(frame.Info.ParentInput ?? "value", frame.Trace);
                else _explanation.Tree = frame.Trace;
            }
            parent = frame;
        }
    }

    internal void EnterNode(int index)
    {
        var info = Program.Nodes[index];
        if (_frames.TryPeek(out var decision) && decision.Trace is not null &&
            decision.Info.Operation == "if" && info.ParentInput is "then" or "else")
            decision.Trace.SelectedBranch = info.ParentInput;
        VmTraceNode? trace = null;
        if (_explanation is not null)
        {
            if (_recordedNodes < _options.MaxTraceSteps)
            {
                _recordedNodes++;
                trace = new(++_executionId, info);
                if (_frames.TryPeek(out var parent) && parent.Trace is not null)
                    parent.Trace.Add(info.ParentInput ?? "value", trace);
                else if (_frames.Count == 0) _explanation.Tree = trace;
            }
            else _explanation.Truncated = true;
        }
        _frames.Push(new(info, trace, Stack.GetCount()));
    }

    internal void ExitNode()
    {
        if (Stack.GetCount() != _frames.Peek().StackBase + 1)
            throw new VmRuntimeFault("NODE_STACK_CONTRACT", "A node must leave exactly one output value on the stack.");
        DynValue result = Stack.Peek();
        if (!VmNodeTypes.Accepts(_frames.Peek().Info.OutputTypes, result))
            throw new VmValueCastException("output:" + string.Join('|', _frames.Peek().Info.OutputTypes), result, _frames.Peek().Info.Operation);
        var snapshot = VmValueSnapshot.Capture(result);
        var frame = _frames.Pop();
        if (frame.Trace is not null)
        {
            frame.Trace.Output = snapshot;
            frame.Trace.Status = "completed";
        }
        if (_frames.TryPeek(out var parent) && frame.Info.ParentInput is string key)
        {
            parent.Values[key] = snapshot;
            if (parent.Trace is not null && parent.Info.Operation == "if" && key is "then" or "else")
                parent.Trace.SelectedBranch = key;
        }
    }

    internal DynValue LoadVariable(string name)
        => _variables.TryGetValue(name, out var value) ? value
            : throw new VmRuntimeFault("VARIABLE_NOT_FOUND", $"Variable '{name}' was not supplied.", name);

    internal DynValue CallFunction(string name, IReadOnlyList<DynValue> args)
    {
        if (!_functions.TryGetValue(name, out var callback))
            throw new VmRuntimeFault("HOST_CALL_FAILED", $"Function '{name}' is not registered.", name);
        return InvokeHost(name, () => callback(args));
    }

    internal DynValue InvokeNode(int index)
    {
        var binding = Program.NodeBindings[index];
        ChargeAllocation(binding.Inputs.Count);
        foreach (var parameter in binding.Parameters.Values) ReserveValue(parameter);
        var arguments = new DynValue[binding.Inputs.Count];
        for (int i = arguments.Length - 1; i >= 0; i--) arguments[i] = Stack.Pull();
        for (int i = 0; i < arguments.Length; i++)
            if (!VmNodeTypes.Accepts(binding.Inputs[i].Definition.AcceptedTypes, arguments[i]))
                throw new VmValueCastException(string.Join('|', binding.Inputs[i].Definition.AcceptedTypes), arguments[i], binding.Definition.Operation);
        var result = InvokeHost(binding.Definition.Operation, () => binding.Callback(new(binding, arguments)));
        if (!VmNodeTypes.Accepts(binding.Definition.Output.Types, result))
            throw new VmValueCastException("output:" + string.Join('|', binding.Definition.Output.Types), result, binding.Definition.Operation);
        return result;
    }

    internal T InvokeHost<T>(string method, Func<T> action)
    {
        try { return action(); }
        catch (Exception error) when (error is not OutOfMemoryException)
        { throw new VmRuntimeFault("HOST_CALL_FAILED", $"Host operation '{method}' failed: {error.Message}", method, error); }
    }

    internal static DynValue CopyConstant(DynValue value) => value.Type switch
    {
        ValueType.Array => DynValue.FromArray(value.AsArray().Select(CopyConstant).ToList()),
        ValueType.Dictionary => DynValue.FromDictionary(value.AsDictionary().ToDictionary(p => CopyConstant(p.Key), p => CopyConstant(p.Value))),
        _ => value
    };

    internal void ChargeAllocation(int units)
    {
        if (units < 0 || (_allocatedValues += units) > _options.MaxAllocatedValues)
            throw new VmRuntimeFault("ALLOCATION_LIMIT", "The script allocation budget was exceeded.");
    }
    internal void CheckCollection(int size)
    {
        if (size < 0 || size > _options.MaxCollectionItems)
            throw new VmRuntimeFault("COLLECTION_LIMIT", "The script collection size limit was exceeded.");
    }
    internal void CheckString(int length)
    {
        if (length < 0 || length > _options.MaxStringLength)
            throw new VmRuntimeFault("STRING_LIMIT", "The script string length limit was exceeded.");
    }
    private void ReserveValue(DynValue value)
    {
        if (value.IsArray)
        {
            var array = value.AsArray(); CheckCollection(array.Count); ChargeAllocation(array.Count + 1);
            foreach (var child in array) ReserveValue(child);
        }
        else if (value.IsDict)
        {
            var dict = value.AsDictionary(); CheckCollection(dict.Count); ChargeAllocation(checked(dict.Count * 2 + 1));
            foreach (var pair in dict) { ReserveValue(pair.Key); ReserveValue(pair.Value); }
        }
        else if (value.Type == ValueType.String) CheckString(value.AsString().Length);
    }
    internal DynValue CopyInvocationConstant(DynValue value)
    { ReserveValue(value); return CopyConstant(value); }

    private void CaptureInstructionInputs(Instruction instruction)
    {
        _instructionInputs.Clear();
        _instructionSnapshots = null;
        void Input(string name, int offset)
        {
            if (offset < Stack.GetCount()) _instructionInputs[name] = Stack.Peek(offset);
        }
        switch (instruction.Opcode)
        {
            case OpcodeId.AssertNodeInput:
                Input(Program.Nodes[instruction.a].InputNames[instruction.b], 0); break;
            case OpcodeId.InvokeNode:
                var binding = Program.NodeBindings[instruction.a];
                foreach (var parameter in binding.Parameters) _instructionInputs[parameter.Key] = parameter.Value;
                for (int i = 0; i < binding.Inputs.Count; i++) Input(binding.Inputs[i].Key, binding.Inputs.Count - 1 - i);
                break;
            case OpcodeId.PushConstant: _instructionInputs["value"] = Program.Constants[instruction.a]; break;
            case OpcodeId.LoadVariable:
                string name = Program.Constants[instruction.a].AsString();
                _instructionInputs["name"] = DynValue.FromString(name);
                if (_variables.TryGetValue(name, out var variable)) _instructionInputs["value"] = variable;
                break;
            case OpcodeId.Add: case OpcodeId.Sub: case OpcodeId.Mul: case OpcodeId.Div: case OpcodeId.Mod:
            case OpcodeId.Eq: case OpcodeId.Neq: case OpcodeId.Gt: case OpcodeId.Lt: case OpcodeId.Ge: case OpcodeId.Le:
            case OpcodeId.Min: case OpcodeId.Max: case OpcodeId.ArrayConcat:
                Input("left", 1); Input("right", 0); break;
            case OpcodeId.Clamp: case OpcodeId.InRange:
                Input("value", 2); Input("min", 1); Input("max", 0); break;
            case OpcodeId.ArrayGet: case OpcodeId.ArrayRemoveAt:
                Input("array", 1); Input("index", 0); break;
            case OpcodeId.ArraySet:
                Input("array", 2); Input("index", 1); Input("value", 0); break;
            case OpcodeId.ArrayPush:
                Input("array", 1); Input("value", 0); break;
            case OpcodeId.ArrayContains:
                if (instruction.b == 1)
                { Input("array", 1); Input("value", 0); _instructionInputs["mode"] = DynValue.FromInt(instruction.a); }
                else { Input("array", 2); Input("value", 1); Input("mode", 0); }
                break;
            case OpcodeId.ArrayLength: Input("array", 0); break;
            case OpcodeId.ObjectGet: case OpcodeId.ObjectHasProperty:
                Input("target", 1); Input("property", 0); break;
            case OpcodeId.ObjectHasMethod: Input("target", 1); Input("method", 0); break;
            case OpcodeId.ObjectSet: Input("target", 2); Input("property", 1); Input("value", 0); break;
            case OpcodeId.ObjectIsValid: case OpcodeId.ObjectFree: case OpcodeId.ObjectGetMethodList: case OpcodeId.ObjectGetPropertyList:
                Input("target", 0); break;
            case OpcodeId.CallFunction:
                _instructionInputs["function"] = Program.Constants[instruction.a];
                for (int i = 0; i < instruction.b; i++) Input("args/" + i, instruction.b - 1 - i);
                break;
            case OpcodeId.ObjectCall:
                int count = instruction.b == 1 ? instruction.a : Stack.IsEmpty() ? 0 : Stack.Peek().AsInt();
                int shift = instruction.b == 1 ? 0 : 1;
                for (int i = 0; i < count; i++) Input("args/" + i, count - 1 - i + shift);
                Input("method", count + shift); Input("target", count + shift + 1); break;
            case OpcodeId.JumpIfFalse: case OpcodeId.JumpIfTrue:
                Input("condition", instruction.b == 1 ? 0 : 1);
                if (instruction.b != 1) Input("target", 0);
                break;
            case OpcodeId.Not: case OpcodeId.Neg: case OpcodeId.AssertBool: case OpcodeId.Pop: case OpcodeId.ExitNode:
                Input("value", 0); break;
        }
        if (instruction.Opcode is OpcodeId.InvokeNode or OpcodeId.CallFunction or OpcodeId.ObjectCall)
            _instructionSnapshots = _instructionInputs.ToDictionary(pair => pair.Key, pair => VmValueSnapshot.Capture(pair.Value));
    }

    private VmExecutionError BuildError(Exception error, int ip, Instruction instruction)
    {
        VmNodeInfo? info = null;
        if ((uint)ip < (uint)Program.Count && Program.Sources[ip] >= 0) info = Program.Nodes[Program.Sources[ip]];
        var source = info?.Source ?? new VmSourceLocation(_script?.SourceFile ?? "<assembled>");
        var values = _frames.TryPeek(out var active)
            ? new Dictionary<string, VmValueSnapshot>(active.Values)
            : new Dictionary<string, VmValueSnapshot>();
        if (info is not null)
            foreach (var name in info.InputNames) values.TryAdd(name, VmValueSnapshot.Unavailable("Input was not evaluated."));
        var inputs = _instructionSnapshots ?? _instructionInputs.ToDictionary(pair => pair.Key, pair => VmValueSnapshot.Capture(pair.Value));
        if (error is VmRuntimeFault { Code: "VARIABLE_NOT_FOUND" })
        {
            inputs["value"] = VmValueSnapshot.Unavailable("Variable was not supplied.");
            values["value"] = inputs["value"];
        }
        var json = new VmJsonInstructionContext(info?.Operation ?? instruction.Opcode.ToString(), values, info?.Path ?? _script?.RootPath ?? "");
        var code = new VmBytecodeInstructionContext(ip, instruction, inputs);
        return error switch
        {
            VmValueCastException cast => new VmTypeMismatchError(cast.ExpectedType, cast.ActualType, cast.Message, source, json, code),
            DivideByZeroException => new VmDivisionByZeroError(source, json, code),
            VmRuntimeFault { Code: "VARIABLE_NOT_FOUND" } missing => new VmVariableNotFoundError(missing.Detail!, source, json, code),
            VmRuntimeFault { Code: "HOST_CALL_FAILED" } host => new VmHostCallError(host.Detail!, host.Message, source, json, code),
            VmRuntimeFault fault => new VmExecutionError(fault.Code, fault.Message, source, json, code),
            IndexOutOfRangeException or ArgumentOutOfRangeException => new VmExecutionError("INDEX_OUT_OF_RANGE", error.Message, source, json, code),
            _ => new VmExecutionError("VM_RUNTIME_ERROR", error.Message, source, json, code)
        };
    }

    private sealed class Frame(VmNodeInfo info, VmTraceNode? trace, int stackBase)
    {
        internal int StackBase { get; } = stackBase;
        internal VmNodeInfo Info { get; } = info;
        internal VmTraceNode? Trace { get; set; } = trace;
        internal Dictionary<string, VmValueSnapshot> Values { get; } = new(info.Parameters);
    }
    private partial int ExecuteInstruction(OpcodeId op, int currentIP);
}
