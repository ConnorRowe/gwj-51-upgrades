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

        public PlayerBody Head { get { return mainBodies[0]; } }

        float impulsePower = 800;
        int partsOnGround = 0;
        float impulseCooldown = 1f;
        float[] timesSinceJumps = new float[3] { 0f, 0f, 0f };
        Vector2[] lastImpulses = new Vector2[3] { Vector2.Zero, Vector2.Zero, Vector2.Zero };
        bool canDoubleJump = true;

        bool queueFreeze = false;
        bool queueUnfreeze = false;
        bool isWet = false;
        float wetness = 0f;
        Vector2 dampJump = new Vector2(-.65f, -.65f);

        public bool InputLocked { get; set; } = false;
        public bool HasPeanutButterUpgrade { get; private set; } = false;
        private bool hasSourdoughUpgrade = false;
        private bool hasBadgeUpgrade = false;

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

            if (SaveData.GameStage >= 2)
            {
                PeanutButterUpgrade();
            }
            if (SaveData.GameStage >= 3)
            {
                SourdoughUpgrade();
            }
            if (SaveData.GameStage >= 4)
            {
                BadgeUpgrade();
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
                wetness += HasPeanutButterUpgrade ? delta * .15f : delta;
                if (wetness > 1f)
                {
                    wetness = 1f;
                    World.KillPlayer(World.DeathType.WATER);
                }
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

            for (int i = 0; i < 3; i++)
            {
                if (timesSinceJumps[i] > 0)
                {
                    timesSinceJumps[i] -= delta;
                    if (timesSinceJumps[i] < 0)
                        timesSinceJumps[i] = 0;
                }
            }
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
            if (InputLocked)
                return;

            if (hasSourdoughUpgrade && canDoubleJump && partsOnGround <= 1)
            {
                bool doubleJumped = false;
                if (evt.IsActionPressed("jump_left"))
                {
                    Vector2 impulse = new Vector2(-.75f, -.25f) * impulsePower;
                    mainBodies[0].ApplyImpulse(new Vector2(1, 1), impulse);
                    mainBodies[5].ApplyImpulse(new Vector2(1, 1), impulse);
                    doubleJumped = true;
                }
                else if (evt.IsActionPressed("jump_right"))
                {
                    Vector2 impulse = new Vector2(.75f, -.25f) * impulsePower;
                    mainBodies[0].ApplyImpulse(new Vector2(-1, 1), impulse);
                    mainBodies[5].ApplyImpulse(new Vector2(-1, 1), impulse);
                    doubleJumped = true;
                }
                else if (evt.IsActionPressed("jump_up"))
                {
                    Vector2 impulse = Vector2.Up * impulsePower;
                    mainBodies[0].ApplyImpulse(Vector2.Zero, impulse);
                    mainBodies[5].ApplyImpulse(Vector2.Zero, impulse);
                    doubleJumped = true;
                }

                if (doubleJumped)
                {
                    World.MakeBigCrumbs(mainBodies[3].GlobalPosition);
                    canDoubleJump = false;
                }

            }

            if (evt.IsActionPressed("jump_left"))
            {
                Vector2 impulse = new Vector2(-.25f, -.75f) * GetImpulsePower() * impulseCooldown;
                lastImpulses[0] = impulse;
                timesSinceJumps[0] = impulseCooldown * .5f;
                impulseCooldown = 0f;
                mainBodies[0].ApplyImpulse(new Vector2(1, 1), impulse);
                mainBodies[5].ApplyImpulse(new Vector2(1, 1), impulse);
            }


            if (evt.IsActionPressed("jump_right"))
            {
                Vector2 impulse = new Vector2(.25f, -.75f) * GetImpulsePower() * impulseCooldown;
                lastImpulses[1] = impulse;
                timesSinceJumps[1] = impulseCooldown * .5f;
                impulseCooldown = 0f;
                mainBodies[0].ApplyImpulse(new Vector2(-1, 1), impulse);
                mainBodies[5].ApplyImpulse(new Vector2(-1, 1), impulse);
            }

            if (evt.IsActionPressed("jump_up"))
            {
                Vector2 impulse = Vector2.Up * GetImpulsePower() * impulseCooldown;
                lastImpulses[2] = impulse;
                timesSinceJumps[2] = impulseCooldown * .5f;
                impulseCooldown = 0f;
                mainBodies[0].ApplyImpulse(Vector2.Zero, impulse);
                mainBodies[5].ApplyImpulse(Vector2.Zero, impulse);
            }

            if (evt.IsActionReleased("jump_left"))
            {
                Vector2 impulse = lastImpulses[0] * dampJump * timesSinceJumps[0];
                mainBodies[0].ApplyImpulse(Vector2.Zero, impulse);
                mainBodies[5].ApplyImpulse(Vector2.Zero, impulse);
            }

            if (evt.IsActionReleased("jump_right"))
            {
                Vector2 impulse = lastImpulses[1] * dampJump * timesSinceJumps[1];
                mainBodies[0].ApplyImpulse(Vector2.Zero, impulse);
                mainBodies[5].ApplyImpulse(Vector2.Zero, impulse);
            }

            if (evt.IsActionReleased("jump_up"))
            {
                Vector2 impulse = lastImpulses[2] * dampJump * timesSinceJumps[2];
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

            if (!canDoubleJump && contactCount >= 4)
                canDoubleJump = true;

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

        public void PeanutButterUpgrade()
        {
            HasPeanutButterUpgrade = true;
            shaderMaterial.SetShaderParam("outline", new Color("bb7547"));
        }

        public void SourdoughUpgrade()
        {
            hasSourdoughUpgrade = true;
        }

        public void BadgeUpgrade()
        {
            hasBadgeUpgrade = true;

            GetNode<Sprite>("Middle3/Badge").Visible = true;
        }

        public void ResetDoubleJump()
        {
            canDoubleJump = true;
        }

    }
}