using System.Runtime.CompilerServices;

namespace VMRuntime;

[Opcode(OpcodeId.JumpIfTrue)]
public class JumpIfTrueOpcode : IOpcode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Emit(Assembler asm, int targetIP)
    {
        asm.AddInstruction(new Instruction
        {
            Opcode = OpcodeId.JumpIfTrue,
            a = targetIP,
            b = 1
        });
    }

    public static void Emit(Assembler asm)
    {
        asm.AddInstruction(new Instruction { Opcode = OpcodeId.JumpIfTrue });
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

        if (condition.AsBool())
            return targetIP;

        return currentIP + 1;
    }
}