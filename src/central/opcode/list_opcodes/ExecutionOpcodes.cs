namespace VMRuntime;

[Opcode(OpcodeId.PushConstant)]
internal sealed class PushConstantOpcode : IOpcode
{
    public static int Execute(VM vm, int ip)
    { vm.Stack.Push(vm.CopyInvocationConstant(vm.Program.Constants[vm.Program[ip].a])); return ip + 1; }
}
[Opcode(OpcodeId.LoadVariable)]
internal sealed class LoadVariableOpcode : IOpcode
{
    public static int Execute(VM vm, int ip)
    { vm.Stack.Push(vm.LoadVariable(vm.Program.Constants[vm.Program[ip].a].AsString())); return ip + 1; }
}
[Opcode(OpcodeId.EnterNode)]
internal sealed class EnterNodeOpcode : IOpcode
{
    public static int Execute(VM vm, int ip) { vm.EnterNode(vm.Program[ip].a); return ip + 1; }
}
[Opcode(OpcodeId.ExitNode)]
internal sealed class ExitNodeOpcode : IOpcode
{
    public static int Execute(VM vm, int ip) { vm.ExitNode(); return ip + 1; }
}
[Opcode(OpcodeId.Pop)]
internal sealed class PopOpcode : IOpcode
{
    public static int Execute(VM vm, int ip) { vm.Stack.Pull(); return ip + 1; }
}
[Opcode(OpcodeId.AssertBool)]
internal sealed class AssertBoolOpcode : IOpcode
{
    public static int Execute(VM vm, int ip) { vm.Stack.Peek().AsBool(); return ip + 1; }
}
[Opcode(OpcodeId.CallFunction)]
internal sealed class CallFunctionOpcode : IOpcode
{
    public static int Execute(VM vm, int ip)
    {
        var instruction = vm.Program[ip];
        vm.ChargeAllocation(instruction.b);
        var args = new DynValue[instruction.b];
        for (int i = args.Length - 1; i >= 0; i--) args[i] = vm.Stack.Pull();
        string name = vm.Program.Constants[instruction.a].AsString();
        vm.Stack.Push(vm.CallFunction(name, Array.AsReadOnly(args)));
        return ip + 1;
    }
}

[Opcode(OpcodeId.InvokeNode)]
internal sealed class InvokeNodeOpcode : IOpcode
{
    public static int Execute(VM vm, int ip)
    { vm.Stack.Push(vm.InvokeNode(vm.Program[ip].a)); return ip + 1; }
}

[Opcode(OpcodeId.AssertNodeInput)]
internal sealed class AssertNodeInputOpcode : IOpcode
{
    public static int Execute(VM vm, int ip)
    {
        var instruction = vm.Program[ip];
        var node = vm.Program.Nodes[instruction.a];
        var value = vm.Stack.Peek();
        if (!VmNodeTypes.Accepts(node.InputTypes[instruction.b], value))
            throw new VmValueCastException(string.Join('|', node.InputTypes[instruction.b]), value,
                node.Operation + "." + node.InputNames[instruction.b]);
        return ip + 1;
    }
}
