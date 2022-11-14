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
        [Export]
        public NodePath TargetCollisionPoly { get; set; }

        public override void _Ready()
        {
            if (TargetCollisionPoly != null && !TargetCollisionPoly.IsEmpty() && GetNodeOrNull(TargetCollisionPoly) is CollisionPolygon2D collisionPolygon2D)
            {
                Polygon = collisionPolygon2D.Polygon;
            }
        }

        public override void _Draw()
        {
            DrawPolyline(Polygon, OutlineColour, OutlineThickness);
            DrawLine(Polygon[Polygon.Length - 1], Polygon[0], OutlineColour, OutlineThickness);
        }
    }
}