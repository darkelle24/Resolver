namespace VMRuntime;

[Opcode(OpcodeId.ObjectSet)]
public sealed class ObjectSetOpcode : IOpcode
{
    /// <summary>Executes this operation through the injected host adapter.</summary>
    public static int Execute(VM vm, int ip)
    {
        var value = vm.Stack.Pull();
        var property = vm.Stack.Pull().AsString();
        var objectValue = vm.Stack.Pull();
        var target = objectValue.AsObject();
        vm.InvokeHost(property, () => { vm.Host.Set(target, property, value); return true; });
        vm.Stack.Push(objectValue);
        return ip + 1;
    }
}
