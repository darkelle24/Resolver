using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VMRuntime;

[Opcode(OpcodeId.ArrayPush)]
public class ArrayPushOpcode : IOpcode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Execute(VM vm, int currentIP)
    {
        DynValue value = vm.Stack.Pull();
        DynValue array = vm.Stack.Pull();
        List<DynValue> list = array.AsArray();
        vm.CheckCollection(checked(list.Count + 1));
        vm.ChargeAllocation(1);
        list.Add(value);
        vm.Stack.Push(array);   // on repousse le tableau pour chaînage
        return currentIP + 1;
    }
}
