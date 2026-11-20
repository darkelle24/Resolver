using System.Runtime.CompilerServices;

namespace VMRuntime;

[Opcode(OpcodeId.Sub)]
public class SubOpcode : IOpcode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Execute(VM vm, int currentIP)
    {
        DynValue b = vm.Stack.Pull();
        DynValue a = vm.Stack.Pull();
        vm.Stack.Push(Operators.Subtract(a, b));
        return currentIP + 1;
    }
}