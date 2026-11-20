using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VMRuntime;

[Opcode(OpcodeId.NewDict)]
public class NewDictOpcode : IOpcode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Execute(VM vm, int currentIP)
    {
        vm.ChargeAllocation(1);
        vm.Stack.Push(DynValue.FromDictionary(new Dictionary<DynValue, DynValue>()));
        return currentIP + 1;
    }
}
