using Godot;
using System;

namespace Bread
{
    public class PlayerBody : RigidBody2D
    {
        static PackedScene crumbsScene = GD.Load<PackedScene>("res://scenes/Crumbs.tscn");
        bool isForcingPosition = false;
        public Vector2 ForcePos { get; private set; } = Vector2.Zero;
        public Vector2 ToasterImpulse { get; set; } = Vector2.Zero;
        Particles2D crumbs;
        Vector2 lastLinearVelocity = Vector2.Zero;

        public override void _Ready()
        {
            crumbs = crumbsScene.Instance<Particles2D>();
            AddChild(crumbs);
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
                bool sharpVelChange = linearVelocityDelta.LengthSquared() > 7000;
                if (sharpVelChange)
                    GD.Print("sharpVelChange");

                crumbs.Emitting = sharpVelChange || (contact && state.LinearVelocity.LengthSquared() > 10f);

                base._IntegrateForces(state);
            }
        }
    }
}