using System.Runtime.CompilerServices;

namespace VMRuntime;

[Opcode(OpcodeId.Not)]
public class NotOpcode : IOpcode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Execute(VM vm, int currentIP)
    {
        DynValue operand = vm.Stack.Pull();
        vm.Stack.Push(Operators.Not(operand));
        return currentIP + 1;
    }
}