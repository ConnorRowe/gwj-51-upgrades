using Godot;

namespace Bread
{
    public class RoomArea : Area2D
    {
        [Export]
        public AudioStreamOGGVorbis MusicTrack { get; set; } = null;
        [Export]
        public Texture BackgroundTexture { get; set; }
        public Vector2 RoomPosition { get; private set; }
        public int LimitLeft { get; private set; }
        public int LimitTop { get; private set; }
        public int LimitRight { get; private set; }
        public int LimitBottom { get; private set; }

        public override void _Ready()
        {
            base._Ready();

            var collision = GetNode<CollisionShape2D>("Bounds");
            RoomPosition = collision.GlobalPosition;
            var bounds = collision.Shape as RectangleShape2D;
            LimitLeft = Mathf.RoundToInt(RoomPosition.x - bounds.Extents.x);
            LimitTop = Mathf.RoundToInt(RoomPosition.y - bounds.Extents.y);
            LimitRight = Mathf.RoundToInt(RoomPosition.x + bounds.Extents.x);
            LimitBottom = Mathf.RoundToInt(RoomPosition.y + bounds.Extents.y);

            Connect("body_entered", this, nameof(BodyEntered));
        }

        protected virtual void BodyEntered(Node body)
        {
            if (body is PlayerBody)
            {
                World.MoveCameraToRoom(this);

                Sounds.PlayMusic(MusicTrack);
            }
        }
    }
}