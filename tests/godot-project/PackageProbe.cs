using VMRuntime.GodotApi;

public partial class PackageProbe : Godot.RefCounted
{
    internal static System.Type EngineType => typeof(VmEngineNode);

    public string ValidateDefaultMethods()
    {
        System.Type[] types = [typeof(Godot.Node), typeof(Godot.Node2D), typeof(Godot.Node3D), typeof(Godot.Control),
            typeof(Godot.Sprite2D), typeof(Godot.AnimatedSprite2D), typeof(Godot.Label),
            typeof(Godot.Button), typeof(Godot.RigidBody2D), typeof(Godot.CharacterBody2D),
            typeof(Godot.Area2D), typeof(Godot.Camera2D), typeof(Godot.Timer),
            typeof(Godot.AudioStreamPlayer), typeof(Godot.AnimationPlayer)];
        foreach (var type in types)
        {
            var target = (Godot.GodotObject)System.Activator.CreateInstance(type)!;
            try
            {
                if (target is Godot.Node2D or Godot.Node3D or Godot.Control)
                    foreach (var name in new[] { "get_position", "set_position", "get_global_position", "set_global_position", "get_rotation", "set_rotation", "get_scale", "set_scale" })
                        if (!VMRuntime.ExposedMethodRegistry.IsMethodAllowed(target, name))
                            return $"{type.Name}: missing transformation permission {name}";
                foreach (var name in VMRuntime.ExposedMethodRegistry.GetAllowedMethods(target))
                    if (!target.HasMethod(name)) return $"{type.Name}: unknown default method {name}";
                if (VMRuntime.ExposedMethodRegistry.IsMethodAllowed(target, "queue_free"))
                    return $"{type.Name}: unlisted method was allowed";
            }
            finally { target.Free(); }
        }
        return "";
    }
}
