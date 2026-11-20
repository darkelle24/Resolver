using System.Runtime.CompilerServices;

namespace VMRuntime;

[Opcode(OpcodeId.Gt)]
public class GtOpcode : IOpcode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Execute(VM vm, int currentIP)
    {
        DynValue b = vm.Stack.Pull();
        DynValue a = vm.Stack.Pull();
        vm.Stack.Push(Operators.GreaterThan(a, b));
        return currentIP + 1;
    }
}