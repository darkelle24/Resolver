using System;
using Godot;

namespace VMRuntime
{
    public static class DefaultExposedMethods
    {
        /// <summary>
        /// Enregistre les méthodes sûres par défaut pour les classes Godot courantes.
        /// </summary>
        public static void RegisterDefaults()
        {
            // ========== Node ==========
            // Identité
            ExposedMethodRegistry.RegisterMethod(typeof(Node), "get_name");
            ExposedMethodRegistry.RegisterMethod(typeof(Node), "set_name");
            // Transformation
            ExposedMethodRegistry.RegisterMethod(typeof(Node), "get_position");
            ExposedMethodRegistry.RegisterMethod(typeof(Node), "set_position");
            ExposedMethodRegistry.RegisterMethod(typeof(Node), "get_global_position");
            ExposedMethodRegistry.RegisterMethod(typeof(Node), "set_global_position");
            ExposedMethodRegistry.RegisterMethod(typeof(Node), "get_rotation");
            ExposedMethodRegistry.RegisterMethod(typeof(Node), "set_rotation");
            ExposedMethodRegistry.RegisterMethod(typeof(Node), "get_scale");
            ExposedMethodRegistry.RegisterMethod(typeof(Node), "set_scale");
            // Visibilité / process
            ExposedMethodRegistry.RegisterMethod(typeof(Node), "get_visible");
            ExposedMethodRegistry.RegisterMethod(typeof(Node), "set_visible");
            ExposedMethodRegistry.RegisterMethod(typeof(Node), "get_process_mode");
            ExposedMethodRegistry.RegisterMethod(typeof(Node), "set_process_mode");

            // ========== Node2D ==========
            // Hérite de Node, mais on peut ajouter des spécificités si besoin
            // (aucune pour l'instant)

            // ========== Control ==========
            ExposedMethodRegistry.RegisterMethod(typeof(Control), "get_rect");
            ExposedMethodRegistry.RegisterMethod(typeof(Control), "set_rect");
            ExposedMethodRegistry.RegisterMethod(typeof(Control), "get_size");
            ExposedMethodRegistry.RegisterMethod(typeof(Control), "set_size");
            ExposedMethodRegistry.RegisterMethod(typeof(Control), "get_position");
            ExposedMethodRegistry.RegisterMethod(typeof(Control), "set_position");
            ExposedMethodRegistry.RegisterMethod(typeof(Control), "get_scale");
            ExposedMethodRegistry.RegisterMethod(typeof(Control), "set_scale");
            ExposedMethodRegistry.RegisterMethod(typeof(Control), "get_rotation");
            ExposedMethodRegistry.RegisterMethod(typeof(Control), "set_rotation");

            // ========== Sprite2D ==========
            ExposedMethodRegistry.RegisterMethod(typeof(Sprite2D), "get_texture");
            ExposedMethodRegistry.RegisterMethod(typeof(Sprite2D), "set_texture");
            ExposedMethodRegistry.RegisterMethod(typeof(Sprite2D), "get_offset");
            ExposedMethodRegistry.RegisterMethod(typeof(Sprite2D), "set_offset");
            ExposedMethodRegistry.RegisterMethod(typeof(Sprite2D), "get_flip_h");
            ExposedMethodRegistry.RegisterMethod(typeof(Sprite2D), "set_flip_h");
            ExposedMethodRegistry.RegisterMethod(typeof(Sprite2D), "get_flip_v");
            ExposedMethodRegistry.RegisterMethod(typeof(Sprite2D), "set_flip_v");

            // ========== AnimatedSprite2D ==========
            ExposedMethodRegistry.RegisterMethod(typeof(AnimatedSprite2D), "get_animation");
            ExposedMethodRegistry.RegisterMethod(typeof(AnimatedSprite2D), "set_animation");
            ExposedMethodRegistry.RegisterMethod(typeof(AnimatedSprite2D), "Play");
            ExposedMethodRegistry.RegisterMethod(typeof(AnimatedSprite2D), "Stop");
            ExposedMethodRegistry.RegisterMethod(typeof(AnimatedSprite2D), "IsPlaying");

            // ========== Label ==========
            ExposedMethodRegistry.RegisterMethod(typeof(Label), "get_text");
            ExposedMethodRegistry.RegisterMethod(typeof(Label), "set_text");
            ExposedMethodRegistry.RegisterMethod(typeof(Label), "get_font_size");
            ExposedMethodRegistry.RegisterMethod(typeof(Label), "set_font_size");

            // ========== Button ==========
            ExposedMethodRegistry.RegisterMethod(typeof(Button), "get_text");
            ExposedMethodRegistry.RegisterMethod(typeof(Button), "set_text");
            ExposedMethodRegistry.RegisterMethod(typeof(Button), "get_pressed");
            ExposedMethodRegistry.RegisterMethod(typeof(Button), "set_pressed");

            // ========== RigidBody2D ==========
            ExposedMethodRegistry.RegisterMethod(typeof(RigidBody2D), "get_linear_velocity");
            ExposedMethodRegistry.RegisterMethod(typeof(RigidBody2D), "set_linear_velocity");
            ExposedMethodRegistry.RegisterMethod(typeof(RigidBody2D), "get_angular_velocity");
            ExposedMethodRegistry.RegisterMethod(typeof(RigidBody2D), "set_angular_velocity");
            ExposedMethodRegistry.RegisterMethod(typeof(RigidBody2D), "ApplyImpulse");
            ExposedMethodRegistry.RegisterMethod(typeof(RigidBody2D), "ApplyForce");

            // ========== CharacterBody2D ==========
            ExposedMethodRegistry.RegisterMethod(typeof(CharacterBody2D), "get_velocity");
            ExposedMethodRegistry.RegisterMethod(typeof(CharacterBody2D), "set_velocity");
            ExposedMethodRegistry.RegisterMethod(typeof(CharacterBody2D), "MoveAndSlide");

            // ========== Area2D ==========
            ExposedMethodRegistry.RegisterMethod(typeof(Area2D), "get_overlapping_bodies");
            ExposedMethodRegistry.RegisterMethod(typeof(Area2D), "get_overlapping_areas");

            // ========== Camera2D ==========
            ExposedMethodRegistry.RegisterMethod(typeof(Camera2D), "get_position");
            ExposedMethodRegistry.RegisterMethod(typeof(Camera2D), "set_position");
            ExposedMethodRegistry.RegisterMethod(typeof(Camera2D), "get_zoom");
            ExposedMethodRegistry.RegisterMethod(typeof(Camera2D), "set_zoom");

            // ========== Timer ==========
            ExposedMethodRegistry.RegisterMethod(typeof(Godot.Timer), "get_time_left");
            ExposedMethodRegistry.RegisterMethod(typeof(Godot.Timer), "Start");
            ExposedMethodRegistry.RegisterMethod(typeof(Godot.Timer), "Stop");
            ExposedMethodRegistry.RegisterMethod(typeof(Godot.Timer), "get_wait_time");
            ExposedMethodRegistry.RegisterMethod(typeof(Godot.Timer), "set_wait_time");

            // ========== AudioStreamPlayer ==========
            ExposedMethodRegistry.RegisterMethod(typeof(AudioStreamPlayer), "Play");
            ExposedMethodRegistry.RegisterMethod(typeof(AudioStreamPlayer), "Stop");
            ExposedMethodRegistry.RegisterMethod(typeof(AudioStreamPlayer), "get_volume_db");
            ExposedMethodRegistry.RegisterMethod(typeof(AudioStreamPlayer), "set_volume_db");

            // ========== AnimationPlayer ==========
            ExposedMethodRegistry.RegisterMethod(typeof(AnimationPlayer), "Play");
            ExposedMethodRegistry.RegisterMethod(typeof(AnimationPlayer), "Stop");
            ExposedMethodRegistry.RegisterMethod(typeof(AnimationPlayer), "get_current_animation");
            ExposedMethodRegistry.RegisterMethod(typeof(AnimationPlayer), "set_current_animation");

            // Ajoutez d'autres classes et méthodes selon vos besoins.
        }
    }
}