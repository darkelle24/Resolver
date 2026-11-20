using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VMRuntime;

[Opcode(OpcodeId.ArraySet)]
public class ArraySetOpcode : IOpcode
{
    // Mode pile (pas d'arguments)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Emit(Assembler asm)
    {
        asm.AddInstruction(new Instruction { Opcode = OpcodeId.ArraySet });
    }

    // Mode immédiat : index + valeur à écrire
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Emit(Assembler asm, int index, int value)
    {
        asm.AddInstruction(new Instruction
        {
            Opcode = OpcodeId.ArraySet,
            a = index,
            b = value,
            c = 1   // indique l'utilisation des champs immédiats
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Execute(VM vm, int currentIP)
    {
        Instruction instr = vm.Program[currentIP];
        DynValue value;
        DynValue index;
        DynValue array;

        if (instr.c == 1) // Mode immédiat : index et valeur sont dans l'instruction
        {
            array = vm.Stack.Pull();          // seul le tableau est sur la pile
            index = DynValue.FromInt(instr.a);
            value = DynValue.FromInt(instr.b);
        }
        else // Mode pile : valeur, index et tableau sont dépilés dans l'ordre
        {
            value = vm.Stack.Pull();
            index = vm.Stack.Pull();
            array = vm.Stack.Pull();
        }

        List<DynValue> list = array.AsArray();
        int idx = index.AsInt();
        list[idx] = value;
        vm.Stack.Push(array);
        return currentIP + 1;
    }
}