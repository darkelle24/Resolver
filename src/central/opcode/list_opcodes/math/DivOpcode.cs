using System.Runtime.CompilerServices;

namespace VMRuntime;

[Opcode(OpcodeId.Div)]
public class DivOpcode : IOpcode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Execute(VM vm, int currentIP)
    {
        DynValue b = vm.Stack.Pull();
        DynValue a = vm.Stack.Pull();
        vm.Stack.Push(Operators.Divide(a, b));
        return currentIP + 1;
    }
}