using Godot;
using System;

namespace Bread
{
    public class Player : Node2D
    {
        static Color bread1 = Colors.Red;
        static Color bread2 = Colors.Blue;
        static Color eyestalkColour = new Color("71413b");
        static Color[] breadColours = new Color[6] { bread1, bread1, bread2, bread2, bread2, bread1 };
        PlayerBody[] mainBodies = new PlayerBody[6];
        Vector2[] bodyPositions = new Vector2[6];
        RigidBody2D left;
        RigidBody2D right;
        Label debugLabel;
        RigidBody2D eyeLeft;
        RigidBody2D eyeRight;
        ShaderMaterial shaderMaterial;
        ShaderMaterial crumbsShaderMat = GD.Load<ShaderMaterial>("res://shaders/CrumbsShaderMat.tres");
        Particles2D crumbsLow;
        Particles2D crumbsMed;
        Particles2D crumbsHigh;

        float impulsePower = 800;
        int partsOnGround = 0;
        float impulseCooldown = 1f;

        bool queueFreeze = false;
        bool queueUnfreeze = false;
        bool isWet = false;
        float wetness = 0f;

        public override void _Ready()
        {
            debugLabel = GetNode<Label>("Middle1/debuglabel");

            crumbsLow = GetNode<Particles2D>("Middle6/CrumbsLow");
            crumbsMed = GetNode<Particles2D>("Middle6/CrumbsMed");
            crumbsHigh = GetNode<Particles2D>("Middle6/CrumbsHigh");

            left = GetNode<RigidBody2D>("Left");
            right = GetNode<RigidBody2D>("Right");
            eyeLeft = GetNode<RigidBody2D>("EyeLeft");
            eyeRight = GetNode<RigidBody2D>("EyeRight");

            shaderMaterial = (ShaderMaterial)Material;

            int bodyCount = 0;

            foreach (Node child in GetChildren())
            {
                if (child.IsInGroup("BreadMain"))
                {
                    mainBodies[bodyCount++] = (PlayerBody)child;
                }
            }
        }

        public override void _Process(float delta)
        {
            Update();

            if (impulseCooldown < 1f)
            {
                impulseCooldown += delta * 3;

                if (impulseCooldown > 1f)
                    impulseCooldown = 1f;
            }

            isWet = false;
            foreach (var body in mainBodies)
            {
                if (body.WetAreasIn.Count > 0)
                {
                    isWet = true;
                    break;
                }
            }

            if (isWet && wetness < 1f)
            {
                wetness += delta;
                if (wetness > 1f)
                    wetness = 1f;
            }
            else if (wetness > 0f)
            {
                wetness -= delta * 2f;
                if (wetness < 0f)
                    wetness = 0f;
            }

            shaderMaterial.SetShaderParam("wetness", wetness);
            crumbsShaderMat.SetShaderParam("wetness", wetness);

            float speed = mainBodies[5].LinearVelocity.LengthSquared();
            crumbsLow.Emitting = speed >= 500 && speed < 5000;
            crumbsMed.Emitting = speed >= 5000 && speed < 16000;
            crumbsHigh.Emitting = speed > 16000;
        }

        public override void _Draw()
        {
            for (int i = 0; i < 6; i++)
            {
                bodyPositions[i] = mainBodies[i].Position;
            }

            var startD = bodyPositions[0].MoveToward(bodyPositions[1], -1) - bodyPositions[0];
            var endD = bodyPositions[5].MoveToward(bodyPositions[1], -1) - bodyPositions[5];

            DrawLine(bodyPositions[0], eyeLeft.Position, eyestalkColour, 1.5f);
            DrawLine(bodyPositions[0], eyeRight.Position, eyestalkColour, 1.5f);

            bodyPositions[0] += startD;
            bodyPositions[5] += endD;
            DrawPolyline(bodyPositions, Colors.Green, 5, true);

            bodyPositions[0] -= startD;
            bodyPositions[5] -= endD;
            DrawPolylineColors(bodyPositions, breadColours, 3, false);
        }

        public override void _Input(InputEvent evt)
        {
            if (evt.IsActionPressed("jump_left"))
            {
                Vector2 impulse = new Vector2(-.25f, -.75f) * GetImpulsePower() * impulseCooldown;
                impulseCooldown = 0f;
                mainBodies[0].ApplyImpulse(new Vector2(1, 1), impulse);
                mainBodies[5].ApplyImpulse(new Vector2(1, 1), impulse);
            }


            if (evt.IsActionPressed("jump_right"))
            {
                Vector2 impulse = new Vector2(.25f, -.75f) * GetImpulsePower() * impulseCooldown;
                impulseCooldown = 0f;
                mainBodies[0].ApplyImpulse(new Vector2(-1, 1), impulse);
                mainBodies[5].ApplyImpulse(new Vector2(-1, 1), impulse);
            }

            if (evt.IsActionPressed("jump_up"))
            {
                Vector2 impulse = Vector2.Up * GetImpulsePower() * impulseCooldown;
                impulseCooldown = 0f;
                mainBodies[0].ApplyImpulse(Vector2.Zero, impulse);
                mainBodies[5].ApplyImpulse(Vector2.Zero, impulse);
            }
        }

        private float GetImpulsePower()
        {
            if (partsOnGround >= 3)
                return impulsePower;
            else
                return impulsePower * ((float)partsOnGround / 3f);
        }

        public override void _PhysicsProcess(float delta)
        {
            if (queueFreeze)
            {
                foreach (var body in mainBodies)
                {
                    body.SetDeferred("mode", RigidBody2D.ModeEnum.Static);
                    //body.Mode = RigidBody2D.ModeEnum.Static;
                }
                //left.Mode = right.Mode = RigidBody2D.ModeEnum.Static;
                left.SetDeferred("mode", RigidBody2D.ModeEnum.Static);
                right.SetDeferred("mode", RigidBody2D.ModeEnum.Static);

                queueFreeze = false;

                return;
            }

            if (queueUnfreeze)
            {
                foreach (var body in mainBodies)
                {
                    //body.Mode = RigidBody2D.ModeEnum.Rigid;
                    body.SetDeferred("mode", RigidBody2D.ModeEnum.Rigid);

                }
                //left.Mode = right.Mode = RigidBody2D.ModeEnum.Rigid;
                left.SetDeferred("mode", RigidBody2D.ModeEnum.Rigid);
                right.SetDeferred("mode", RigidBody2D.ModeEnum.Rigid);

                queueUnfreeze = false;

                return;
            }

            int contactCount = 0;
            foreach (var body in mainBodies)
            {
                foreach (var contact in body.GetCollidingBodies())
                {
                    if (!(contact is PlayerBody))
                    {
                        contactCount++;
                        break;
                    }
                }
            }

            partsOnGround = contactCount;
        }

        public void ForceBodiesPos(Vector2 globalPosition, float rotation)
        {
            mainBodies[0].ForcePosition(globalPosition);

            for (int i = 1; i < 6; i++)
            {
                mainBodies[i].ForcePosition(mainBodies[i - 1].ForcePos + new Vector2(0, 2.1f).Rotated(rotation));
            }

            left.GlobalPosition = mainBodies[2].GlobalPosition + new Vector2(-5, 1).Rotated(rotation);
            right.GlobalPosition = mainBodies[2].GlobalPosition + new Vector2(5, 1).Rotated(rotation);
        }

        public void ReleaseBodies()
        {
            foreach (var body in mainBodies)
            {
                body.ReleaseForcePosition();
            }
        }

        public void ApplyImpulse(Vector2 impulse)
        {
            foreach (var body in mainBodies)
            {
                body.ToasterImpulse = impulse;
            }
        }

    }
}