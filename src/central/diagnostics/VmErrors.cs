namespace VMRuntime.Diagnostics;

/// <summary>RFC 6901 source pointer; line and UTF-8 byte column are one-based, or both zero when unknown.</summary>
public sealed class VmSourceLocation : VmJsonObject
{
    public string File { get; }
    public string Path { get; }
    public long Line { get; }
    public long ByteColumn { get; }
    public bool HasPosition => Line > 0 && ByteColumn > 0;
    /// <summary>Creates a location in the original document; root JSON Pointer is an empty string.</summary>
    public VmSourceLocation(string file, string path = "", long line = 1, long byteColumn = 1)
        => (File, Path, Line, ByteColumn) = (file, path, line, byteColumn);
}

/// <summary>Common root for compilation and execution diagnostics.</summary>
public abstract class VmError : VmJsonObject
{
    public abstract string Kind { get; }
    public string Code { get; }
    public string Message { get; }
    public VmSourceLocation Source { get; }
    protected VmError(string code, string message, VmSourceLocation source)
        => (Code, Message, Source) = (code, message, source);
}

/// <summary>A failure before a program can execute.</summary>
public class VmCompilationError : VmError
{
    public override string Kind => "compilation";
    /// <summary>Creates a source-aware compilation diagnostic.</summary>
    public VmCompilationError(string code, string message, VmSourceLocation source) : base(code, message, source) { }
}

/// <summary>Malformed JSON syntax.</summary>
public sealed class VmJsonSyntaxError : VmCompilationError
{
    /// <summary>Creates a JSON syntax diagnostic.</summary>
    public VmJsonSyntaxError(string message, VmSourceLocation source) : base("JSON_SYNTAX", message, source) { }
}

/// <summary>A required field was absent.</summary>
public sealed class VmMissingFieldError : VmCompilationError
{
    public string Field { get; }
    /// <summary>Creates a diagnostic at the containing object, with the missing field name.</summary>
    public VmMissingFieldError(string field, VmSourceLocation source)
        : base("JSON_MISSING_FIELD", $"Required field '{field}' is missing.", source) => Field = field;
}

/// <summary>Flat values associated with the JSON operation active when execution failed.</summary>
public sealed class VmJsonInstructionContext : VmJsonObject
{
    public string Operation { get; }
    /// <summary>Logical position in the merged document; Source.Path identifies the original file location.</summary>
    public string Path { get; }
    public IReadOnlyDictionary<string, VmValueSnapshot> Inputs { get; }
    /// <summary>Captures operation parameters and evaluated input values.</summary>
    public VmJsonInstructionContext(string operation, IReadOnlyDictionary<string, VmValueSnapshot> inputs, string path = "")
        => (Operation, Inputs, Path) = (operation, inputs, path);
}

/// <summary>The failing bytecode instruction and its actual operands, captured before stack mutation.</summary>
public sealed class VmBytecodeInstructionContext : VmJsonObject
{
    public int InstructionIndex { get; }
    public string Opcode { get; }
    public IReadOnlyDictionary<string, int> Operands { get; }
    public IReadOnlyDictionary<string, VmValueSnapshot> Inputs { get; }
    /// <summary>Creates a diagnostic for a zero-based instruction index.</summary>
    public VmBytecodeInstructionContext(int index, Instruction instruction, IReadOnlyDictionary<string, VmValueSnapshot> inputs)
    {
        InstructionIndex = index;
        Opcode = index < 0 ? "Unavailable" : instruction.Opcode.ToString();
        Operands = new Dictionary<string, int> { ["a"] = instruction.a, ["b"] = instruction.b, ["c"] = instruction.c };
        Inputs = inputs;
    }
}

/// <summary>A runtime failure, with local JSON and bytecode context even when explanation is disabled.</summary>
public class VmExecutionError : VmError
{
    public override string Kind => "execution";
    public VmJsonInstructionContext JsonInstruction { get; }
    public VmBytecodeInstructionContext BytecodeInstruction { get; }
    /// <summary>Creates a runtime diagnostic with both levels of execution context.</summary>
    public VmExecutionError(string code, string message, VmSourceLocation source,
        VmJsonInstructionContext jsonInstruction, VmBytecodeInstructionContext bytecodeInstruction)
        : base(code, message, source) => (JsonInstruction, BytecodeInstruction) = (jsonInstruction, bytecodeInstruction);
}

/// <summary>A script requested a variable absent from the invocation context.</summary>
public sealed class VmVariableNotFoundError : VmExecutionError
{
    public string VariableName { get; }
    internal VmVariableNotFoundError(string name, VmSourceLocation source, VmJsonInstructionContext json, VmBytecodeInstructionContext code)
        : base("VARIABLE_NOT_FOUND", $"Variable '{name}' was not supplied.", source, json, code) => VariableName = name;
}

/// <summary>A checked accessor or operation received an incompatible type.</summary>
public sealed class VmTypeMismatchError : VmExecutionError
{
    public string ExpectedType { get; }
    public string ActualType { get; }
    internal VmTypeMismatchError(string expected, string actual, string message, VmSourceLocation source,
        VmJsonInstructionContext json, VmBytecodeInstructionContext code)
        : base("TYPE_MISMATCH", message, source, json, code) => (ExpectedType, ActualType) = (expected, actual);
}

/// <summary>An integer division or modulo had a zero divisor.</summary>
public sealed class VmDivisionByZeroError : VmExecutionError
{
    internal VmDivisionByZeroError(VmSourceLocation source, VmJsonInstructionContext json, VmBytecodeInstructionContext code)
        : base("DIVISION_BY_ZERO", "An integer divisor was zero.", source, json, code) { }
}

/// <summary>A registered host callback or object adapter failed.</summary>
public sealed class VmHostCallError : VmExecutionError
{
    public string Method { get; }
    internal VmHostCallError(string method, string message, VmSourceLocation source,
        VmJsonInstructionContext json, VmBytecodeInstructionContext code)
        : base("HOST_CALL_FAILED", message, source, json, code) => Method = method;
}

internal sealed class VmCompileFault(VmCompilationError error) : Exception(error.Message)
{
    internal VmCompilationError Error { get; } = error;
}
internal sealed class VmRuntimeFault(string code, string message, string? detail = null, Exception? inner = null)
    : Exception(message, inner)
{
    internal string Code { get; } = code;
    internal string? Detail { get; } = detail;
}
