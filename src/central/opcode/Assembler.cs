namespace VMRuntime;

/// <summary>Low-level bytecode assembler. Prefer VmEngine for source-aware JSON programs.</summary>
public partial class Assembler
{
    private readonly Program _program;
    /// <summary>Creates an assembler that appends to the supplied mutable program.</summary>
    public Assembler(Program program) => _program = program ?? throw new ArgumentNullException(nameof(program));
    internal void AddInstruction(Instruction instruction) => _program.Add(instruction);
    /// <summary>Appends compiled source-aware bytecode, relocating constants, branches and source bindings.</summary>
    public void Append(CompiledScript script)
    {
        ArgumentNullException.ThrowIfNull(script);
        _program.Append(script);
    }
}
