using VMRuntime;

public static class Operators
{
    // Instances uniques (thread‑safe car créées une seule fois)
    private static readonly AdditionOperator _add = new();
    private static readonly SubtractionOperator _sub = new();
    private static readonly MultiplicationOperator _mul = new();
    private static readonly DivisionOperator _div = new();
    private static readonly ModuloOperator _mod = new();
    private static readonly EqualOperator _eq = new();
    private static readonly NotEqualOperator _neq = new();
    private static readonly GreaterThanOperator _gt = new();
    private static readonly LessThanOperator _lt = new();
    private static readonly GreaterOrEqualOperator _ge = new();
    private static readonly LessOrEqualOperator _le = new();
    private static readonly NotOperator _not = new();
    private static readonly NegateOperator _neg = new();

    // ─── Opérations binaires ──────────────────────────────────────
    public static DynValue Add(in DynValue left, in DynValue right) => _add.Resolve(left, right);
    public static DynValue Subtract(in DynValue left, in DynValue right) => _sub.Resolve(left, right);
    public static DynValue Multiply(in DynValue left, in DynValue right) => _mul.Resolve(left, right);
    public static DynValue Divide(in DynValue left, in DynValue right) => _div.Resolve(left, right);
    public static DynValue Modulo(in DynValue left, in DynValue right) => _mod.Resolve(left, right);
    public static DynValue Equal(in DynValue left, in DynValue right) => _eq.Resolve(left, right);
    public static DynValue NotEqual(in DynValue left, in DynValue right) => _neq.Resolve(left, right);
    public static DynValue GreaterThan(in DynValue left, in DynValue right) => _gt.Resolve(left, right);
    public static DynValue LessThan(in DynValue left, in DynValue right) => _lt.Resolve(left, right);
    public static DynValue GreaterOrEqual(in DynValue left, in DynValue right) => _ge.Resolve(left, right);
    public static DynValue LessOrEqual(in DynValue left, in DynValue right) => _le.Resolve(left, right);

    // ─── Opérations unaires ───────────────────────────────────────
    public static DynValue Not(in DynValue operand) => _not.Resolve(operand);
    public static DynValue Negate(in DynValue operand) => _neg.Resolve(operand);
}