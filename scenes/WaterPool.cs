using Godot;
using System;

namespace Bread
{
    [Tool]
    public class WaterPool : Area2D
    {
        Vector2 size = new Vector2(16, 16);
        [Export]
        public Vector2 Size
        {
            get { return size; }
            set
            {
                if (value != size)
                {
                    size = value;
                    UpdateWater();
                }
            }
        }
        ShaderMaterial spriteMat;
        const float FrameTime = 1f / 6f;
        float count = 0;
        int[] frames = { 0, 1, 2, 2, 1, 0 };
        int frame = 0;

        public override void _Ready()
        {
            if (Engine.EditorHint)
                return;

            spriteMat = GetNode<Sprite>("Sprite").Material as ShaderMaterial;

            Connect("body_entered", this, nameof(BodyEntered));
            Connect("body_exited", this, nameof(BodyExited));
        }

        public override void _Process(float delta)
        {
            if (Engine.EditorHint)
                return;

            count += delta;
            if (count >= FrameTime)
            {
                count -= FrameTime;

                frame++;
                if (frame >= frames.Length)
                    frame = 0;

                spriteMat.SetShaderParam("frame", frame);
            }
        }

        public void UpdateWater()
        {
            var sprite = GetNode<Sprite>("Sprite");
            sprite.RegionRect = new Rect2(0, 0, size.x * 3, 3);
            sprite.Position = new Vector2(0, -3);

            var fill = GetNode<Sprite>("Fill");
            fill.RegionRect = new Rect2(Vector2.Zero, size);

            var collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
            (collisionShape.Shape as RectangleShape2D).Extents = new Vector2(size.x / 2, (size.y / 2) + 1);
            collisionShape.Position = new Vector2(size.x / 2, (size.y / 2) - 1);
        }

        private void BodyEntered(Node body)
        {
            if (body is PlayerBody playerBody)
            {
                playerBody.WetAreasIn.Add(this);
                playerBody.AddPool(this);
            }
        }

        private void BodyExited(Node body)
        {
            if (body is PlayerBody playerBody)
            {
                playerBody.WetAreasIn.Remove(this);
                playerBody.RemovePool(this);
            }
        }
    }
}