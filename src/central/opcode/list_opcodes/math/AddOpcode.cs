using System.Runtime.CompilerServices;

namespace VMRuntime;

[Opcode(OpcodeId.Add)]
public class AddOpcode : IOpcode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Execute(VM vm, int currentIP)
    {
        DynValue b = vm.Stack.Pull();
        DynValue a = vm.Stack.Pull();
        if (a.Type == ValueType.String && b.Type == ValueType.String)
        {
            int length = checked(a.AsString().Length + b.AsString().Length);
            vm.CheckString(length); vm.ChargeAllocation(length);
        }
        var result = Operators.Add(a, b);
        if (result.Type == ValueType.String)
        {
            vm.CheckString(result.AsString().Length);
            if (a.Type != ValueType.String || b.Type != ValueType.String) vm.ChargeAllocation(result.AsString().Length);
        }
        vm.Stack.Push(result);
        return currentIP + 1;
    }
}
