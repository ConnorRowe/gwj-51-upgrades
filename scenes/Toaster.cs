using Godot;
using System;

namespace Bread
{
    public class Toaster : Area2D
    {
        const float CooldownTime = 1f;

        AnimationPlayer animationPlayer;
        Vector2 playerMovePos;
        bool pushedDown = false;
        float cooldown = 0;

        bool leftHeld = false;
        bool rightHeld = false;

        public override void _Ready()
        {
            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            playerMovePos = GetNode<Position2D>("PlayerMovePos").GlobalPosition;
            Connect("body_entered", this, nameof(BodyEntered));
        }

        public override void _Process(float delta)
        {
            if (cooldown > 0 && !pushedDown)
            {
                cooldown -= delta;
                if (cooldown < 0)
                    cooldown = 0;
            }
        }

        public override void _Input(InputEvent evt)
        {
            if (evt.IsActionPressed("jump_left"))
                leftHeld = true;
            if (evt.IsActionReleased("jump_left"))
                leftHeld = false;
            if (evt.IsActionPressed("jump_right"))
                rightHeld = true;
            if (evt.IsActionReleased("jump_right"))
                rightHeld = false;
        }

        private void BodyEntered(Node body)
        {
            if (!pushedDown && cooldown <= 0 && body.Owner is Player player)
            {
                GD.Print("Capturing player and pushing down toaster.");

                player.ForceBodiesPos(playerMovePos, Rotation);

                //player.Freeze();

                animationPlayer.Play("PushDown");

                pushedDown = true;

                GetTree().CreateTimer(1f).Connect("timeout", this, nameof(Launch), new Godot.Collections.Array() { player });

                cooldown = CooldownTime;
            }
        }

        private void Launch(Player player)
        {
            pushedDown = false;

            animationPlayer.Play("Launch");
            if (IsInstanceValid(player))
            {
                player.ReleaseBodies();

                float inputRotate = 0f;
                if (leftHeld)
                    inputRotate -= 1f;
                if (rightHeld)
                    inputRotate += 1f;

                var imp = Vector2.Up.Rotated(Rotation).Rotated(inputRotate * Mathf.Pi * .0625f) * 500f;
                player.ApplyImpulse(imp);

                GD.Print("Launching player with ", imp);
            }
        }
    }
}