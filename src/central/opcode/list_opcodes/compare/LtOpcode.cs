using System.Runtime.CompilerServices;

namespace VMRuntime;

[Opcode(OpcodeId.Lt)]
public class LtOpcode : IOpcode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Execute(VM vm, int currentIP)
    {
        DynValue b = vm.Stack.Pull();
        DynValue a = vm.Stack.Pull();
        vm.Stack.Push(Operators.LessThan(a, b));
        return currentIP + 1;
    }
}