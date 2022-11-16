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
        Particles2D crumbs;
        Vector2 lastLinearVelocity = Vector2.Zero;

        public override void _Ready()
        {
            crumbs = crumbsScene.Instance<Particles2D>();
            crumbs.ZIndex = 5;
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
                float velDeltaLenSq = linearVelocityDelta.LengthSquared();
                bool sharpVelChange = velDeltaLenSq > 8500;
                if (velDeltaLenSq > 2500)
                {
                    GD.Print("sharpVelChange ", velDeltaLenSq);
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
    }
}