using System.Runtime.CompilerServices;

namespace VMRuntime;

[Opcode(OpcodeId.Eq)]
public class EqOpcode : IOpcode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Execute(VM vm, int currentIP)
    {
        DynValue b = vm.Stack.Pull();
        DynValue a = vm.Stack.Pull();
        vm.Stack.Push(Operators.Equal(a, b));
        return currentIP + 1;
    }
}