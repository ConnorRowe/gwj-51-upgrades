using Godot;

namespace Bread
{
    public class MyButton : Godot.Button
    {
        public override void _Ready()
        {
            Connect("mouse_entered", this, nameof(MouseEntered));
        }

        void MouseEntered()
        {
            Sounds.Blip();
        }
    }
}