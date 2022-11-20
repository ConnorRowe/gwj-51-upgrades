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

        public override void _Ready()
        {
            activeSprite = GetNode<AnimatedSprite>("ActiveSprite");
            inactiveSprite = GetNode<Sprite>("InactiveSprite");
            Connect("body_entered", this, nameof(BodyEntered));
        }

        void BodyEntered(Node body)
        {
            if (body is PlayerBody)
            {
                IsActive = true;
                World.CheckpointActivated(this);
                World.MakeSmokePuff(GlobalPosition);
                Sounds.Checkpoint();
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