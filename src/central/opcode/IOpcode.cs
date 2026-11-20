namespace VMRuntime;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class OpcodeAttribute : Attribute
{
    public OpcodeId Id { get; }

    public OpcodeAttribute(OpcodeId id)
    {
        Id = id;
    }
}
public interface IOpcode
{
    static abstract int Execute(VM vm, int currentIP);
}