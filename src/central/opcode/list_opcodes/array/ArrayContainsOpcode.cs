using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace VMRuntime;

[Opcode(OpcodeId.ArrayContains)]
public class ArrayContainsOpcode : IOpcode
{
    // Mode pile : le mode (0 = Any, 1 = All) est dépilé de la pile
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Emit(Assembler asm)
    {
        asm.AddInstruction(new Instruction { Opcode = OpcodeId.ArrayContains });
    }

    // Mode immédiat : le mode est encodé dans l'instruction (champ a)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Emit(Assembler asm, int mode)
    {
        asm.AddInstruction(new Instruction
        {
            Opcode = OpcodeId.ArrayContains,
            a = mode,
            b = 1   // drapeau mode immédiat
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Execute(VM vm, int currentIP)
    {
        Instruction instr = vm.Program[currentIP];

        int mode;
        DynValue needles;
        DynValue haystack;

        // Récupération du mode : immédiat ou pile
        if (instr.b == 1)
        {
            mode = instr.a;
            // Ordre d'empilement attendu : haystack, puis needles
            needles = vm.Stack.Pull();
            haystack = vm.Stack.Pull();
        }
        else
        {
            // Mode pile : mode, needles, haystack (mode au sommet)
            mode = vm.Stack.Pull().AsInt();
            needles = vm.Stack.Pull();
            haystack = vm.Stack.Pull();
        }

        // Vérification du haystack
        var haystackList = haystack.AsArray();
        if (haystackList == null)
        {
            vm.Stack.Push(DynValue.FromBool(false));
            return currentIP + 1;
        }

        bool result;

        if (needles.IsArray)
        {
            var needlesList = needles.AsArray();
            // needles est un tableau
            bool modeAny = (mode == 0);
            result = modeAny
                ? needlesList.Any(needle => haystackList.Any(h => Operators.Equal(h, needle).AsBool()))
                : needlesList.All(needle => haystackList.Any(h => Operators.Equal(h, needle).AsBool()));
        }
        else
        {
            // needles est une valeur simple
            result = haystackList.Any(h => Operators.Equal(h, needles).AsBool());
        }

        vm.Stack.Push(DynValue.FromBool(result));
        return currentIP + 1;
    }
}
