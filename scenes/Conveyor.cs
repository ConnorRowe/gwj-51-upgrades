using Godot;
using System.Collections.Generic;

namespace Bread
{
    [Tool]
    public class Conveyor : StaticBody2D, IButtonInteractive
    {
        const float MoveSpeed = 230;
        float length = 10;
        [Export]
        public float Length
        {
            get { return length; }
            set
            {
                if (value != length)
                {
                    length = value;
                    UpdateConveyor();
                }
            }
        }

        [Export]
        public float Speed { get; set; } = 1;
        [Export]
        public bool Reverse { get; set; } = false;

        ShaderMaterial spriteShader;
        List<RigidBody2D> detectedBodies = new List<RigidBody2D>();

        public override void _Ready()
        {
            if (!Engine.EditorHint)
            {
                var timer = GetNode<Timer>("AnimTimer");
                timer.WaitTime /= Speed;
                timer.Connect("timeout", this, nameof(AnimTimeout));
                spriteShader = (ShaderMaterial)GetNode<Sprite>("Sprite").Material;

                var detectArea = GetNode<Area2D>("DetectArea");
                detectArea.Connect("body_entered", this, nameof(BodyEntered));
                detectArea.Connect("body_exited", this, nameof(BodyExited));
            }
        }

        private void AnimTimeout()
        {
            int frame = (int)spriteShader.GetShaderParam("frame");
            frame += Reverse ? -1 : 1;
            if (frame > 9)
                frame = 0;
            if (frame < 0)
                frame = 9;

            spriteShader.SetShaderParam("frame", frame);
        }

        public void BodyEntered(Node body)
        {
            if (body is RigidBody2D rigidBody2D)
            {
                detectedBodies.Add(rigidBody2D);
            }
        }

        public void BodyExited(Node body)
        {
            if (body is RigidBody2D rigidBody2D)
            {
                detectedBodies.Remove(rigidBody2D);
            }
        }

        public void UpdateConveyor()
        {
            var sprite = GetNode<Sprite>("Sprite");
            sprite.RegionRect = new Rect2(0, 0, length * 10, 6);
            sprite.Position = new Vector2(length / 2, 0);

            var collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
            (collisionShape.Shape as RectangleShape2D).Extents = new Vector2(length / 2, 3);
            collisionShape.Position = sprite.Position;

            var detectArea = GetNode<Area2D>("DetectArea");
            detectArea.Position = new Vector2(length / 2, -6);
        }

        public override void _PhysicsProcess(float delta)
        {
            foreach (var body in detectedBodies)
            {
                body.ApplyImpulse(Vector2.Down.Rotated(Rotation), Vector2.Right.Rotated(Rotation) * MoveSpeed * Speed * delta * (Reverse ? -1f : 1f));
            }
        }

        public void ButtonInteract()
        {
            Reverse = !Reverse;
        }
    }
}