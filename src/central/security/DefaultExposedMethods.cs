using Godot;

namespace VMRuntime;

/// <summary>Default native Godot permissions, shared by all engine instances.</summary>
public static class DefaultExposedMethods
{
    /// <summary>Registers native SDK names on the types that provide them.</summary>
    public static void RegisterDefaults()
    {
        Register<Node>(Node.MethodName.GetName, Node.MethodName.SetName, Node.MethodName.GetProcessMode, Node.MethodName.SetProcessMode);
        Register<Node2D>(Node2D.MethodName.GetPosition, Node2D.MethodName.SetPosition, Node2D.MethodName.GetGlobalPosition, Node2D.MethodName.SetGlobalPosition, Node2D.MethodName.GetRotation, Node2D.MethodName.SetRotation, Node2D.MethodName.GetScale, Node2D.MethodName.SetScale);
        Register<CanvasItem>(CanvasItem.MethodName.IsVisible, CanvasItem.MethodName.SetVisible);
        Register<Node3D>(Node3D.MethodName.GetPosition, Node3D.MethodName.SetPosition, Node3D.MethodName.GetGlobalPosition, Node3D.MethodName.SetGlobalPosition, Node3D.MethodName.GetRotation, Node3D.MethodName.SetRotation, Node3D.MethodName.GetScale, Node3D.MethodName.SetScale, Node3D.MethodName.IsVisible, Node3D.MethodName.SetVisible);
        Register<Control>(Control.MethodName.GetRect, Control.MethodName.GetSize, Control.MethodName.SetSize, Control.MethodName.GetPosition, Control.MethodName.SetPosition, Control.MethodName.GetGlobalPosition, Control.MethodName.SetGlobalPosition, Control.MethodName.GetScale, Control.MethodName.SetScale, Control.MethodName.GetRotation, Control.MethodName.SetRotation);
        Register<Sprite2D>(Sprite2D.MethodName.GetTexture, Sprite2D.MethodName.SetTexture, Sprite2D.MethodName.GetOffset, Sprite2D.MethodName.SetOffset, Sprite2D.MethodName.IsFlippedH, Sprite2D.MethodName.SetFlipH, Sprite2D.MethodName.IsFlippedV, Sprite2D.MethodName.SetFlipV);
        Register<AnimatedSprite2D>(AnimatedSprite2D.MethodName.GetAnimation, AnimatedSprite2D.MethodName.SetAnimation, AnimatedSprite2D.MethodName.Play, AnimatedSprite2D.MethodName.Stop, AnimatedSprite2D.MethodName.IsPlaying);
        Register<Label>(Label.MethodName.GetText, Label.MethodName.SetText);
        Register<Button>(Button.MethodName.GetText, Button.MethodName.SetText, Button.MethodName.IsPressed, Button.MethodName.SetPressed);
        Register<RigidBody2D>(RigidBody2D.MethodName.GetLinearVelocity, RigidBody2D.MethodName.SetLinearVelocity, RigidBody2D.MethodName.GetAngularVelocity, RigidBody2D.MethodName.SetAngularVelocity, RigidBody2D.MethodName.ApplyImpulse, RigidBody2D.MethodName.ApplyForce);
        Register<CharacterBody2D>(CharacterBody2D.MethodName.GetVelocity, CharacterBody2D.MethodName.SetVelocity, CharacterBody2D.MethodName.MoveAndSlide);
        Register<Area2D>(Area2D.MethodName.GetOverlappingBodies, Area2D.MethodName.GetOverlappingAreas);
        Register<Camera2D>(Camera2D.MethodName.GetZoom, Camera2D.MethodName.SetZoom);
        Register<Godot.Timer>(Godot.Timer.MethodName.GetTimeLeft, Godot.Timer.MethodName.Start, Godot.Timer.MethodName.Stop, Godot.Timer.MethodName.GetWaitTime, Godot.Timer.MethodName.SetWaitTime);
        Register<AudioStreamPlayer>(AudioStreamPlayer.MethodName.Play, AudioStreamPlayer.MethodName.Stop, AudioStreamPlayer.MethodName.GetVolumeDb, AudioStreamPlayer.MethodName.SetVolumeDb);
        Register<AnimationPlayer>(AnimationPlayer.MethodName.Play, AnimationPlayer.MethodName.Stop, AnimationPlayer.MethodName.GetCurrentAnimation, AnimationPlayer.MethodName.SetCurrentAnimation);
    }

    private static void Register<T>(params StringName[] methods) where T : GodotObject
    {
        foreach (var method in methods)
            ExposedMethodRegistry.RegisterMethod(typeof(T), method.ToString());
    }
}
