using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VMRuntime;

[Opcode(OpcodeId.ArrayRemoveAt)]
public class ArrayRemoveAtOpcode : IOpcode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Emit(Assembler asm)
    {
        asm.AddInstruction(new Instruction { Opcode = OpcodeId.ArrayRemoveAt });
    }

    // Immédiat
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Emit(Assembler asm, int index)
    {
        asm.AddInstruction(new Instruction
        {
            Opcode = OpcodeId.ArrayRemoveAt,
            a = index,
            b = 1   // mode immédiat
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Execute(VM vm, int currentIP)
    {
        Instruction instr = vm.Program[currentIP];
        DynValue index;
        DynValue array;

        if (instr.b == 1) // Mode immédiat : seul le tableau est sur la pile
        {
            array = vm.Stack.Pull();
            index = DynValue.FromInt(instr.a);
        }
        else // Mode pile : index puis tableau dépilés
        {
            index = vm.Stack.Pull();
            array = vm.Stack.Pull();
        }

        List<DynValue> list = array.AsArray();
        int idx = index.AsInt();
        list.RemoveAt(idx);
        vm.Stack.Push(array);
        return currentIP + 1;
    }
}