using System.Runtime.CompilerServices;

namespace VMRuntime;

[Opcode(OpcodeId.JumpIfFalse)]
public class JumpIfFalseOpcode : IOpcode
{
    // Mode immédiat : la cible est dans l'instruction
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Emit(Assembler asm, int targetIP)
    {
        asm.AddInstruction(new Instruction
        {
            Opcode = OpcodeId.JumpIfFalse,
            a = targetIP,
            b = 1
        });
    }

    // Mode pile (moins fréquent) : la cible est dépilée puis la condition
    public static void Emit(Assembler asm)
    {
        asm.AddInstruction(new Instruction { Opcode = OpcodeId.JumpIfFalse });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Execute(VM vm, int currentIP)
    {
        Instruction instr = vm.Program[currentIP];
        int targetIP;
        DynValue condition;

        if (instr.b == 1)
        {
            targetIP = instr.a;
            condition = vm.Stack.Pull();
        }
        else
        {
            targetIP = vm.Stack.Pull().AsInt();
            condition = vm.Stack.Pull();
        }

        if (!condition.AsBool())
            return targetIP;

        return currentIP + 1;
    }
}