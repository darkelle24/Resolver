namespace VMRuntime;

[Opcode(OpcodeId.ObjectGetPropertyList)]
public sealed class ObjectGetPropertyListOpcode : IOpcode
{
    /// <summary>Executes this operation through the injected host adapter.</summary>
    public static int Execute(VM vm, int ip)
    {
        var target = vm.Stack.Pull().AsObject();
        var names = vm.InvokeHost("get_property_list", () => vm.Host.GetProperties(target));
        vm.Stack.Push(DynValue.FromArray(names.Select(DynValue.FromString).ToList()));
        return ip + 1;
    }
}
