using System.Runtime.CompilerServices;

namespace VMRuntime;

[Opcode(OpcodeId.Neg)]
public class NegOpcode : IOpcode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Execute(VM vm, int currentIP)
    {
        DynValue operand = vm.Stack.Pull();
        vm.Stack.Push(Operators.Negate(operand));
        return currentIP + 1;
    }
}