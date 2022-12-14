using Godot;

namespace Bread
{
    [Tool]
    public class Raincloud : Area2D
    {
        float height = 20;
        [Export]
        public float Height
        {
            get { return height; }
            set { if (height != value) { height = value; UpdateRaincloud(); } }
        }

        float particleLifetime = 0;
        [Export]
        public float ParticleLifetime
        {
            get
            {

                if (IsInsideTree() || Engine.EditorHint)
                {
                    try
                    {
                        return GetNode<Particles2D>("Sprite/Particles2D").Lifetime;
                    }
                    catch
                    {
                        return particleLifetime;
                    }
                }
                else
                    return particleLifetime;
            }
            set
            {
                try
                {
                    if (IsInsideTree() || Engine.EditorHint)
                        GetNode<Particles2D>("Sprite/Particles2D").Lifetime = value;
                }
                catch
                {
                    GD.Print("oopsie :)");
                }

                particleLifetime = value;
            }
        }

        private void UpdateRaincloud()
        {
            var collisionShape2D = GetNode<CollisionShape2D>("CollisionShape2D");
            collisionShape2D.Position = new Vector2(0, -(height / 2));
            (collisionShape2D.Shape as RectangleShape2D).Extents = new Vector2(10, height / 2);
            GetNode<Sprite>("Sprite").Position = new Vector2(0, -height);
        }

        public override void _Ready()
        {
            if (Engine.EditorHint)
                return;

            GetNode<Particles2D>("Sprite/Particles2D").Lifetime = particleLifetime;

            Connect("body_entered", this, nameof(BodyEntered));
            Connect("body_exited", this, nameof(BodyExited));
        }

        private void BodyEntered(Node body)
        {
            if (body is PlayerBody playerBody)
                playerBody.WetAreasIn.Add(this);

        }

        private void BodyExited(Node body)
        {
            if (body is PlayerBody playerBody)
                playerBody.WetAreasIn.Remove(this);
        }
    }
}