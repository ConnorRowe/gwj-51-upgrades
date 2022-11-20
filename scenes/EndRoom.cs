using Godot;
using System;

namespace Bread
{
    public class EndRoom : RoomArea
    {
        Sprite lightning;

        public override void _Ready()
        {
            base._Ready();
            lightning = GetNode<Sprite>("Lightning");
            SetProcess(false);
        }

        void LockPlayer()
        {
            World.Player.InputLocked = true;
        }

        void SetPlayerEyesVisible(bool visible)
        {
            World.Player.GetNode<Node2D>("EyeLeft").Visible = visible;
            World.Player.GetNode<Node2D>("EyeRight").Visible = visible;
            World.Player.GetNode<Node2D>("PupilLeft").Visible = visible;
            World.Player.GetNode<Node2D>("PupilRight").Visible = visible;

            World.Player.Modulate = visible ? Colors.White : Colors.DimGray;
        }

        protected override void BodyEntered(Node body)
        {
            base.BodyEntered(body);

            if (body is PlayerBody)
            {
                GetNode<AnimationPlayer>("AnimationPlayer").Play("end");
                GetNode<CanvasLayer>("CanvasLayer2").Visible = true;
                SetProcess(true);
            }
        }

        public override void _Process(float delta)
        {
            lightning.GlobalPosition = World.Player.Head.GlobalPosition + new Vector2(0, -80);
        }

        void QuitGame()
        {
            GetTree().ChangeScene("res://scenes/menus/EndScreen.tscn");
            QueueFree();
        }
    }
}