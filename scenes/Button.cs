using Godot;

namespace Bread
{
    public class Button : StaticBody2D
    {
        RigidBody2D plate;
        ShaderMaterial plateShader;
        bool triggered = false;

        [Export]
        public NodePath TargetNodePath { get; set; } = "";

        IButtonInteractive target;

        public override void _Ready()
        {
            plate = GetNode<RigidBody2D>("PlateBody");
            plateShader = GetNode<Sprite>("PlateBody/Sprite").Material as ShaderMaterial;
            plateShader.SetShaderParam("y_cutoff_pos", Position.y);
            target = GetNodeOrNull<IButtonInteractive>(TargetNodePath);
        }

        public override void _PhysicsProcess(float delta)
        {
            plateShader.SetShaderParam("global_transform", plate.GlobalTransform);

            if (!triggered && plate.Position.y >= -2)
            {
                Trigger();
            }
            else if (triggered && plate.Position.y <= -4)
            {
                Reset();
            }
        }

        private void Trigger()
        {
            triggered = true;
            target.ButtonInteract();
        }

        private void Reset()
        {
            triggered = false;
        }
    }
}