using System.Runtime.CompilerServices;

namespace VMRuntime;

[Opcode(OpcodeId.Jump)]
public class JumpOpcode : IOpcode
{
    // ── Émission (utilisée par l'assembleur) ──────────────────
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Emit(Assembler asm, int targetAddress)
    {
        asm.AddInstruction(new Instruction
        {
            Opcode = OpcodeId.Jump,
            a = targetAddress
        });
    }

    // ── Exécution (appelée par le dispatch généré) ────────────
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Execute(VM vm, int currentIP)
    {
        // L'instruction contient l'adresse cible absolue dans le champ "a"
        Instruction instr = vm.Program[currentIP];
        return instr.a;
    }
}