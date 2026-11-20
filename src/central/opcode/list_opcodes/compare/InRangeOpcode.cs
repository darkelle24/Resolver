using System.Runtime.CompilerServices;

namespace VMRuntime;

[Opcode(OpcodeId.InRange)]
public class InRangeOpcode : IOpcode
{
    // Pile : max, min, value
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Execute(VM vm, int currentIP)
    {
        var max = vm.Stack.Pull();
        var min = vm.Stack.Pull();
        var value = vm.Stack.Pull();

        bool inRange = Operators.LessOrEqual(min, value).AsBool() &&
                       Operators.LessOrEqual(value, max).AsBool();
        vm.Stack.Push(DynValue.FromBool(inRange));
        return currentIP + 1;
    }
}