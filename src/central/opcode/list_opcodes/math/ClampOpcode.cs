using System.Runtime.CompilerServices;

namespace VMRuntime;

[Opcode(OpcodeId.Clamp)]
public class ClampOpcode : IOpcode
{
    // Opération purement pile : max, min, value
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Execute(VM vm, int currentIP)
    {
        var max = vm.Stack.Pull();
        var min = vm.Stack.Pull();
        var value = vm.Stack.Pull();

        DynValue result;
        if (Operators.LessThan(value, min).AsBool())
            result = min;
        else if (Operators.GreaterThan(value, max).AsBool())
            result = max;
        else
            result = value;

        vm.Stack.Push(result);
        return currentIP + 1;
    }
}