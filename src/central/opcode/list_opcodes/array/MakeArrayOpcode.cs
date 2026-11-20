using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VMRuntime;

[Opcode(OpcodeId.MakeArray)]
public class MakeArrayOpcode : IOpcode
{
    // Mode pile : count est au sommet de la pile
    public static void Emit(Assembler asm)
    {
        asm.AddInstruction(new Instruction { Opcode = OpcodeId.MakeArray });
    }

    // Mode immédiat : count est encodé dans l'instruction
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Emit(Assembler asm, int count)
    {
        asm.AddInstruction(new Instruction
        {
            Opcode = OpcodeId.MakeArray,
            a = count,
            b = 1   // drapeau immédiat
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Execute(VM vm, int currentIP)
    {
        Instruction instr = vm.Program[currentIP];
        int count;

        if (instr.b == 1)
        {
            count = instr.a;
        }
        else
        {
            count = vm.Stack.Pull().AsInt();
        }

        vm.CheckCollection(count);
        vm.ChargeAllocation(checked(count + 1));
        var array = new List<DynValue>(count);
        for (int i = 0; i < count; i++)
            array.Add(vm.Stack.Pull());      // Pull pour retirer dans l'ordre inverse
        array.Reverse();                    // Rétablir l'ordre d'origine
        vm.Stack.Push(DynValue.FromArray(array));
        return currentIP + 1;
    }
}
