using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VMRuntime;

[Opcode(OpcodeId.NewArray)]
public class NewArrayOpcode : IOpcode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Execute(VM vm, int currentIP)
    {
        vm.ChargeAllocation(1);
        vm.Stack.Push(DynValue.FromArray(new List<DynValue>()));
        return currentIP + 1;
    }
}
