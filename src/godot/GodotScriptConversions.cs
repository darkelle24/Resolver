#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
namespace VMRuntime;

/// <summary>Component copies between independent shims and Godot; the core assembly stays identical in all hosts.</summary>
public static class GodotScriptConversions
{
    /// <summary>Copies a Godot Vector2 into a Float32 script vector.</summary>
    public static ScriptVector2 ToScript(this Vector2 v) => new((float)v.X, (float)v.Y);
    /// <summary>Copies a script vector into Godot.</summary>
    public static Vector2 ToGodot(this ScriptVector2 v) => new(v.X, v.Y);
    /// <summary>Copies a Godot Vector2I into a script vector.</summary>
    public static ScriptVector2I ToScript(this Vector2I v) => new(v.X, v.Y);
    /// <summary>Copies an integer script vector into Godot.</summary>
    public static Vector2I ToGodot(this ScriptVector2I v) => new(v.X, v.Y);
    /// <summary>Copies a Godot Vector3 into a Float32 script vector.</summary>
    public static ScriptVector3 ToScript(this Vector3 v) => new((float)v.X, (float)v.Y, (float)v.Z);
    /// <summary>Copies a script Vector3 into Godot.</summary>
    public static Vector3 ToGodot(this ScriptVector3 v) => new(v.X, v.Y, v.Z);
    /// <summary>Copies a Godot Vector3I into a script vector.</summary>
    public static ScriptVector3I ToScript(this Vector3I v) => new(v.X, v.Y, v.Z);
    /// <summary>Copies an integer script Vector3 into Godot.</summary>
    public static Vector3I ToGodot(this ScriptVector3I v) => new(v.X, v.Y, v.Z);
    /// <summary>Copies a Godot color into the script representation.</summary>
    public static ScriptColor ToScript(this Color c) => new(c.R, c.G, c.B, c.A);
    /// <summary>Copies a script color into Godot.</summary>
    public static Color ToGodot(this ScriptColor c) => new(c.R, c.G, c.B, c.A);
}
