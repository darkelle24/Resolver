namespace VMRuntime;

[Opcode(OpcodeId.ObjectGet)]
public sealed class ObjectGetOpcode : IOpcode
{
    /// <summary>Executes this operation through the injected host adapter.</summary>
    public static int Execute(VM vm, int ip)
    {
        var property = vm.Stack.Pull().AsString();
        var target = vm.Stack.Pull().AsObject();
        vm.Stack.Push(vm.InvokeHost(property, () => vm.Host.Get(target, property)));
        return ip + 1;
    }
}
