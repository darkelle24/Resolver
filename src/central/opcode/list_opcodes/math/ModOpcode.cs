using System.Runtime.CompilerServices;

namespace VMRuntime;

[Opcode(OpcodeId.Mod)]
public class ModOpcode : IOpcode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Execute(VM vm, int currentIP)
    {
        DynValue b = vm.Stack.Pull();
        DynValue a = vm.Stack.Pull();
        vm.Stack.Push(Operators.Modulo(a, b));
        return currentIP + 1;
    }
}