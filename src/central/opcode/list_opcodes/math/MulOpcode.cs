using System.Runtime.CompilerServices;

namespace VMRuntime;

[Opcode(OpcodeId.Mul)]
public class MulOpcode : IOpcode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Execute(VM vm, int currentIP)
    {
        DynValue b = vm.Stack.Pull();
        DynValue a = vm.Stack.Pull();
        vm.Stack.Push(Operators.Multiply(a, b));
        return currentIP + 1;
    }
}