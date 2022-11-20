using Godot;
using System;

namespace Bread
{
    [Tool]
    public class TerrainPolygon2D : Polygon2D
    {
        const float OutlineThickness = 2f;

        [Export]
        public Godot.Color OutlineColour { get; set; } = Colors.Black;

        public override void _Ready()
        {
            if (!Engine.EditorHint)
            {
                var collision = new CollisionPolygon2D();
                collision.Polygon = Polygon;
                GetParent().CallDeferred("add_child", collision);
                collision.Position = Position;
            }
        }

        public override void _Draw()
        {
            DrawPolyline(Polygon, OutlineColour, OutlineThickness);
            DrawLine(Polygon[Polygon.Length - 1], Polygon[0], OutlineColour, OutlineThickness);
        }
    }
}