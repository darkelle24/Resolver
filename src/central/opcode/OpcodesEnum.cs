// OpcodesEnum.cs
namespace VMRuntime;

public enum OpcodeId : byte
{
    // math
    Add,
    Clamp,
    Min,
    Max,
    Sub,
    Mul,
    Div,
    Mod,
    // compare
    Eq,
    Neq,
    Gt,
    Lt,
    Ge,
    Le,
    InRange,
    // unary
    Not,
    Neg,
    // control
    Jump,
    JumpIfFalse,
    JumpIfTrue,
    // array
    NewArray,
    ArrayPush,
    ArrayGet,
    ArraySet,
    ArrayLength,
    ArrayRemoveAt,
    ArrayContains,
    ArrayConcat,
    MakeArray,
    //Dictionary
    NewDict,
    //Object
    ObjectGet,
    ObjectSet,
    ObjectCall,
    ObjectIsValid,
    ObjectFree,
    ObjectHasMethod,
    ObjectHasProperty,
    ObjectGetPropertyList,
    ObjectGetMethodList,
    PushConstant,
    LoadVariable,
    EnterNode,
    ExitNode,
    Pop,
    CallFunction,
    AssertBool,
    InvokeNode,
    AssertNodeInput
}
