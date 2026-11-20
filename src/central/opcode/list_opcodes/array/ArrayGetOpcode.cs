using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VMRuntime;

[Opcode(OpcodeId.ArrayGet)]
public class ArrayGetOpcode : IOpcode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Emit(Assembler asm)
    {
        asm.AddInstruction(new Instruction { Opcode = OpcodeId.ArrayGet });
    }

    // Version avec index immédiat (optimisation)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Emit(Assembler asm, int index)
    {
        asm.AddInstruction(new Instruction
        {
            Opcode = OpcodeId.ArrayGet,
            a = index,
            b = 1   // drapeau pour indiquer le mode immédiat
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Execute(VM vm, int currentIP)
    {
        // Récupération de l'instruction pour connaître le mode (immédiat ou pile)
        Instruction instr = vm.Program[currentIP];
        DynValue index;
        DynValue array;

        if (instr.b == 1) // Mode immédiat : seul le tableau est sur la pile
        {
            array = vm.Stack.Pull();
            index = DynValue.FromInt(instr.a);
        }
        else // Mode standard : index et tableau sont dépilés
        {
            index = vm.Stack.Pull();
            array = vm.Stack.Pull();
        }

        List<DynValue> list = array.AsArray();
        int idx = index.AsInt();
        vm.Stack.Push(list[idx]);
        return currentIP + 1;
    }
}