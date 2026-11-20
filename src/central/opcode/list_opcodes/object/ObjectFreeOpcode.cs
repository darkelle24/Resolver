namespace VMRuntime;

[Opcode(OpcodeId.ObjectFree)]
public sealed class ObjectFreeOpcode : IOpcode
{
    /// <summary>Executes this operation through the injected host adapter.</summary>
    public static int Execute(VM vm, int ip)
    {
        var target = vm.Stack.Pull().AsObject();
        vm.InvokeHost("free", () => { vm.Host.Free(target); return true; });
        return ip + 1;
    }
}
