using Godot;
using System.Collections.Generic;

namespace Bread
{
    public class PlayerBody : RigidBody2D
    {
        static PackedScene crumbsScene = GD.Load<PackedScene>("res://scenes/Crumbs.tscn");
        bool isForcingPosition = false;
        public Vector2 ForcePos { get; private set; } = Vector2.Zero;
        public Vector2 ToasterImpulse { get; set; } = Vector2.Zero;
        public List<Node> WetAreasIn { get; } = new List<Node>();
        List<Node> poolsIn { get; } = new List<Node>();
        Particles2D crumbs;
        Vector2 lastLinearVelocity = Vector2.Zero;
        bool isUnderwater = false;
        Player player;
        bool canSwim = true;

        public override void _Ready()
        {
            player = GetParent<Player>();
            crumbs = crumbsScene.Instance<Particles2D>();
            crumbs.ZIndex = 5;
            AddChild(crumbs);
        }

        public override void _Input(InputEvent evt)
        {
            if (isUnderwater && player.HasPeanutButterUpgrade && canSwim)
            {
                var swimDir = Vector2.Zero;

                if (evt.IsActionPressed("jump_up"))
                    swimDir = Vector2.Up;
                else if (evt.IsActionPressed("jump_left"))
                    swimDir = Vector2.Left;
                else if (evt.IsActionPressed("jump_right"))
                    swimDir = Vector2.Right;
                else if (evt.IsActionPressed("down"))
                    swimDir = Vector2.Down;

                if (swimDir != Vector2.Zero)
                {
                    ApplyImpulse(swimDir, swimDir * 60);
                    canSwim = false;

                    GetTree().CreateTimer(.5f).Connect("timeout", this, nameof(ResetCanSwim));
                }
            }
        }



        public void ForcePosition(Vector2 globalPosition)
        {
            isForcingPosition = true;
            ForcePos = globalPosition;
        }

        public void ReleaseForcePosition()
        {
            isForcingPosition = false;
        }

        public void ResetCanSwim()
        {
            canSwim = true;
        }

        public override void _IntegrateForces(Physics2DDirectBodyState state)
        {
            if (isForcingPosition)
            {
                GlobalPosition = ForcePos;
                state.Transform = new Transform2D(Rotation, ForcePos);
            }
            else
            {
                if (ToasterImpulse != Vector2.Zero)
                {
                    state.ApplyCentralImpulse(ToasterImpulse);
                    ToasterImpulse = Vector2.Zero;
                }

                bool contact = false;

                for (int i = 0; i < state.GetContactCount(); i++)
                {
                    if (!(state.GetContactColliderObject(i) is PlayerBody))
                    {
                        contact = true;
                        break;
                    }
                }

                var linearVelocityDelta = state.LinearVelocity - lastLinearVelocity;
                lastLinearVelocity = state.LinearVelocity;
                float velDeltaLenSq = linearVelocityDelta.LengthSquared();
                bool sharpVelChange = velDeltaLenSq > 8500;
                if (velDeltaLenSq > 2500)
                {
                    if (velDeltaLenSq <= 20000)
                        Sounds.SoftSlap(Mathf.Clamp(((velDeltaLenSq / 20000) * .55f) + .25f, 0f, 0.85f));
                    else if (velDeltaLenSq <= 55000)
                        Sounds.MedSlap();
                    else
                        Sounds.HardSlap();
                }

                crumbs.Emitting = sharpVelChange || (contact && state.LinearVelocity.LengthSquared() > 10f);

                crumbs.GlobalRotation = 0f;

                base._IntegrateForces(state);
            }
        }

        public void AddPool(Node pool)
        {
            poolsIn.Add(pool);

            CheckIsUnderwater();
        }

        public void RemovePool(Node pool)
        {
            poolsIn.Remove(pool);

            CheckIsUnderwater();
        }

        void CheckIsUnderwater()
        {
            isUnderwater = poolsIn.Count > 0;
        }
    }
}