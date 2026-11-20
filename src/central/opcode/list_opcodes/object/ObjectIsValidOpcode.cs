namespace VMRuntime;

[Opcode(OpcodeId.ObjectIsValid)]
public sealed class ObjectIsValidOpcode : IOpcode
{
    /// <summary>Executes this operation through the injected host adapter.</summary>
    public static int Execute(VM vm, int ip)
    {
        var value = vm.Stack.Pull();
        bool valid = !value.IsNull && vm.InvokeHost("is_valid", () => vm.Host.IsValid(value.AsObject()));
        vm.Stack.Push(DynValue.FromBool(valid));
        return ip + 1;
    }
}
