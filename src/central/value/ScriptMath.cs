using System.Runtime.InteropServices;
namespace VMRuntime;
/// <summary>Immutable engine-independent ScriptVector2 value; arithmetic is component-wise.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct ScriptVector2(float X, float Y)
{
    /// <summary>Applies + to corresponding components.</summary>
    public static ScriptVector2 operator +(ScriptVector2 a, ScriptVector2 b) => new(a.X + b.X, a.Y + b.Y);
    /// <summary>Applies - to corresponding components.</summary>
    public static ScriptVector2 operator -(ScriptVector2 a, ScriptVector2 b) => new(a.X - b.X, a.Y - b.Y);
    /// <summary>Applies * to corresponding components.</summary>
    public static ScriptVector2 operator *(ScriptVector2 a, ScriptVector2 b) => new(a.X * b.X, a.Y * b.Y);
    /// <summary>Applies / to corresponding components.</summary>
    public static ScriptVector2 operator /(ScriptVector2 a, ScriptVector2 b) => new(a.X / b.X, a.Y / b.Y);
    /// <summary>Negates every component.</summary>
    public static ScriptVector2 operator -(ScriptVector2 value) => new(-value.X, -value.Y);
}
/// <summary>Immutable engine-independent ScriptVector2I value; arithmetic is component-wise.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct ScriptVector2I(int X, int Y)
{
    /// <summary>Applies + to corresponding components.</summary>
    public static ScriptVector2I operator +(ScriptVector2I a, ScriptVector2I b) => new(a.X + b.X, a.Y + b.Y);
    /// <summary>Applies - to corresponding components.</summary>
    public static ScriptVector2I operator -(ScriptVector2I a, ScriptVector2I b) => new(a.X - b.X, a.Y - b.Y);
    /// <summary>Applies * to corresponding components.</summary>
    public static ScriptVector2I operator *(ScriptVector2I a, ScriptVector2I b) => new(a.X * b.X, a.Y * b.Y);
    /// <summary>Applies / to corresponding components.</summary>
    public static ScriptVector2I operator /(ScriptVector2I a, ScriptVector2I b) => new(a.X / b.X, a.Y / b.Y);
    /// <summary>Negates every component.</summary>
    public static ScriptVector2I operator -(ScriptVector2I value) => new(-value.X, -value.Y);
}
/// <summary>Immutable engine-independent ScriptVector3 value; arithmetic is component-wise.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct ScriptVector3(float X, float Y, float Z)
{
    /// <summary>Applies + to corresponding components.</summary>
    public static ScriptVector3 operator +(ScriptVector3 a, ScriptVector3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    /// <summary>Applies - to corresponding components.</summary>
    public static ScriptVector3 operator -(ScriptVector3 a, ScriptVector3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    /// <summary>Applies * to corresponding components.</summary>
    public static ScriptVector3 operator *(ScriptVector3 a, ScriptVector3 b) => new(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
    /// <summary>Applies / to corresponding components.</summary>
    public static ScriptVector3 operator /(ScriptVector3 a, ScriptVector3 b) => new(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
    /// <summary>Negates every component.</summary>
    public static ScriptVector3 operator -(ScriptVector3 value) => new(-value.X, -value.Y, -value.Z);
}
/// <summary>Immutable engine-independent ScriptVector3I value; arithmetic is component-wise.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct ScriptVector3I(int X, int Y, int Z)
{
    /// <summary>Applies + to corresponding components.</summary>
    public static ScriptVector3I operator +(ScriptVector3I a, ScriptVector3I b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    /// <summary>Applies - to corresponding components.</summary>
    public static ScriptVector3I operator -(ScriptVector3I a, ScriptVector3I b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    /// <summary>Applies * to corresponding components.</summary>
    public static ScriptVector3I operator *(ScriptVector3I a, ScriptVector3I b) => new(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
    /// <summary>Applies / to corresponding components.</summary>
    public static ScriptVector3I operator /(ScriptVector3I a, ScriptVector3I b) => new(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
    /// <summary>Negates every component.</summary>
    public static ScriptVector3I operator -(ScriptVector3I value) => new(-value.X, -value.Y, -value.Z);
}
/// <summary>Immutable engine-independent ScriptColor value; arithmetic is component-wise.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct ScriptColor(float R, float G, float B, float A = 1f)
{
    /// <summary>Applies + to corresponding components.</summary>
    public static ScriptColor operator +(ScriptColor a, ScriptColor b) => new(a.R + b.R, a.G + b.G, a.B + b.B, a.A + b.A);
    /// <summary>Applies - to corresponding components.</summary>
    public static ScriptColor operator -(ScriptColor a, ScriptColor b) => new(a.R - b.R, a.G - b.G, a.B - b.B, a.A - b.A);
    /// <summary>Applies * to corresponding components.</summary>
    public static ScriptColor operator *(ScriptColor a, ScriptColor b) => new(a.R * b.R, a.G * b.G, a.B * b.B, a.A * b.A);
    /// <summary>Applies / to corresponding components.</summary>
    public static ScriptColor operator /(ScriptColor a, ScriptColor b) => new(a.R / b.R, a.G / b.G, a.B / b.B, a.A / b.A);
    /// <summary>Negates every component.</summary>
    public static ScriptColor operator -(ScriptColor value) => new(-value.R, -value.G, -value.B, -value.A);
}
