using System.Runtime.CompilerServices;

namespace VMRuntime;

[Opcode(OpcodeId.Max)]
public class MaxOpcode : IOpcode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Execute(VM vm, int currentIP)
    {
        var b = vm.Stack.Pull();
        var a = vm.Stack.Pull();

        DynValue result = Operators.GreaterThan(a, b).AsBool() ? a : b;
        vm.Stack.Push(result);
        return currentIP + 1;
    }
}