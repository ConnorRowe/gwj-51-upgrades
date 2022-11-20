using Godot;
using System;

namespace Bread
{
    public class Door : StaticBody2D, IButtonInteractive
    {
        [Export]
        bool startOpen = false;

        bool isOpen = false;
        SceneTreeTween doorTween = null;

        KinematicBody2D door;
        CollisionShape2D doorCollision;
        RectangleShape2D doorShape;
        ShaderMaterial doorMat;

        public void ButtonInteract()
        {
            if (doorTween != null)
            {
                doorTween.Kill();
                doorTween.Dispose();
            }

            doorTween = CreateTween();
            doorTween.SetParallel();

            doorTween.TweenProperty(door, "position", new Vector2(0, isOpen ? 0 : -20), .5f);
            doorTween.TweenProperty(doorCollision, "position", new Vector2(0, isOpen ? 15.5f : 25.5f), .5f);
            doorTween.TweenProperty(doorShape, "extents", new Vector2(7, isOpen ? 15.5f : 5.5f), .5f);

            isOpen = !isOpen;
        }

        public override void _Ready()
        {
            door = GetNode<KinematicBody2D>("DoorBody");
            doorCollision = door.GetNode<CollisionShape2D>("CollisionShape2D");
            doorShape = doorCollision.Shape as RectangleShape2D;
            doorMat = GetNode<Sprite>("DoorBody/Sprite").Material as ShaderMaterial;
            doorMat.SetShaderParam("y_cutoff_pos", GlobalPosition.y);

            if (startOpen)
            {
                isOpen = true;
                door.Position = new Vector2(0, -20);
                doorCollision.Position = new Vector2(0, 25.5f);
                doorShape.Extents = new Vector2(7, 5.5f);
            }
        }

        public override void _Process(float delta)
        {
            doorMat.SetShaderParam("global_transform", door.GlobalTransform);
        }
    }
}