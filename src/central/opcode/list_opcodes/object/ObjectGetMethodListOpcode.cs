namespace VMRuntime;

[Opcode(OpcodeId.ObjectGetMethodList)]
public sealed class ObjectGetMethodListOpcode : IOpcode
{
    /// <summary>Executes this operation through the injected host adapter.</summary>
    public static int Execute(VM vm, int ip)
    {
        var target = vm.Stack.Pull().AsObject();
        var names = vm.InvokeHost("get_method_list", () => vm.Host.GetMethods(target));
        vm.Stack.Push(DynValue.FromArray(names.Select(DynValue.FromString).ToList()));
        return ip + 1;
    }
}
