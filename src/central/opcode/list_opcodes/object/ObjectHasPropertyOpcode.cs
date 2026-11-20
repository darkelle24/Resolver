namespace VMRuntime;

[Opcode(OpcodeId.ObjectHasProperty)]
public sealed class ObjectHasPropertyOpcode : IOpcode
{
    /// <summary>Executes this operation through the injected host adapter.</summary>
    public static int Execute(VM vm, int ip)
    {
        var name = vm.Stack.Pull().AsString();
        var target = vm.Stack.Pull().AsObject();
        bool found = vm.InvokeHost(name, () => vm.Host.GetProperties(target).Contains(name, StringComparer.Ordinal));
        vm.Stack.Push(DynValue.FromBool(found));
        return ip + 1;
    }
}
