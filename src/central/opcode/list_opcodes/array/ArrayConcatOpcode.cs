using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VMRuntime;

[Opcode(OpcodeId.ArrayConcat)]
public class ArrayConcatOpcode : IOpcode
{
    // Concatène deux tableaux (listes) – pile : array2, array1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Execute(VM vm, int currentIP)
    {
        var array2 = vm.Stack.Pull();
        var array1 = vm.Stack.Pull();

        var list1 = array1.AsArray();
        var list2 = array2.AsArray();
        int size = checked(list1.Count + list2.Count);
        vm.CheckCollection(size);
        vm.ChargeAllocation(size + 1);

        // if (list1 == null || list2 == null)
        //     throw new VMError("Concat: both arguments must be arrays.");

        var result = new List<DynValue>(size);
        result.AddRange(list1);
        result.AddRange(list2);

        vm.Stack.Push(DynValue.FromArray(result));
        return currentIP + 1;
    }
}
