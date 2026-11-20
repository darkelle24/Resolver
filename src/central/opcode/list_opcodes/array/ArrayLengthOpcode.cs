using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VMRuntime;

[Opcode(OpcodeId.ArrayLength)]
public class ArrayLengthOpcode : IOpcode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Execute(VM vm, int currentIP)
    {
        DynValue array = vm.Stack.Pull();
        int len = array.AsArray().Count;
        vm.Stack.Push(DynValue.FromInt(len));
        return currentIP + 1;
    }
}