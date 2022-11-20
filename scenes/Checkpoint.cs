using Godot;
using System;

namespace Bread
{
    public class Checkpoint : Area2D
    {
        bool isActive = false;
        public bool IsActive
        {
            get { return isActive; }
            set
            {
                if (isActive != value)
                {
                    isActive = value;
                    UpdateCheckpoint();
                }
            }
        }
        AnimatedSprite activeSprite;
        Sprite inactiveSprite;
        Shaker shaker;

        public override void _Ready()
        {
            activeSprite = GetNode<AnimatedSprite>("ActiveSprite");
            inactiveSprite = GetNode<Sprite>("InactiveSprite");
            Connect("body_entered", this, nameof(BodyEntered));
            shaker = GetNode<Shaker>("Shaker");
        }

        void BodyEntered(Node body)
        {
            if (body is PlayerBody)
            {
                IsActive = true;
                World.CheckpointActivated(this);
                World.MakeSmokePuff(GlobalPosition);
                Sounds.Checkpoint();
                World.MakeSpeechBubble("im a bread boy :)", World.Player.Head, new Vector2(0, -8));
                shaker.Shake(1);
            }
        }

        void UpdateCheckpoint()
        {
            activeSprite.Visible = isActive;
            inactiveSprite.Visible = !isActive;
            SetDeferred("monitoring", !isActive);
        }
    }
}