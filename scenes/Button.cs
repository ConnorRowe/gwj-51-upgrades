using Godot;

namespace Bread
{
    public class Button : StaticBody2D
    {
        RigidBody2D plate;
        ShaderMaterial plateShader;
        AudioStreamPlayer2D audioStreamPlayer2D;
        bool triggered = false;

        [Export]
        public NodePath TargetNodePath { get; set; } = "";

        IButtonInteractive target;

        public override void _Ready()
        {
            plate = GetNode<RigidBody2D>("PlateBody");
            plateShader = GetNode<Sprite>("PlateBody/Sprite").Material as ShaderMaterial;
            plateShader.SetShaderParam("y_cutoff_pos", GlobalPosition.y);
            target = GetNodeOrNull<IButtonInteractive>(TargetNodePath);
            audioStreamPlayer2D = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
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
            audioStreamPlayer2D.Play();
        }

        private void Reset()
        {
            triggered = false;
        }
    }
}