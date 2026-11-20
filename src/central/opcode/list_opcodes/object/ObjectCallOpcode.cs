namespace VMRuntime;

[Opcode(OpcodeId.ObjectCall)]
public sealed class ObjectCallOpcode : IOpcode
{
    /// <summary>Executes this operation through the injected host adapter.</summary>
    public static int Execute(VM vm, int ip)
    {
        var instruction = vm.Program[ip];
        int count = instruction.b == 1 ? instruction.a : vm.Stack.Pull().AsInt();
        if (count < 0 || count > vm.Stack.GetCount() - 2) throw new ArgumentOutOfRangeException(nameof(count));
        var arguments = new DynValue[count];
        for (int i = count - 1; i >= 0; i--) arguments[i] = vm.Stack.Pull();
        string method = vm.Stack.Pull().AsString();
        object target = vm.Stack.Pull().AsObject();
        vm.Stack.Push(vm.InvokeHost(method, () => vm.Host.Call(target, method, Array.AsReadOnly(arguments))));
        return ip + 1;
    }
    /// <summary>Emits a call with an immediate argument count.</summary>
    public static void Emit(Assembler assembler, int count)
        => assembler.AddInstruction(new() { Opcode = OpcodeId.ObjectCall, a = count, b = 1 });
    /// <summary>Emits a call whose argument count is on top of the stack.</summary>
    public static void Emit(Assembler assembler)
        => assembler.AddInstruction(new() { Opcode = OpcodeId.ObjectCall });
}
