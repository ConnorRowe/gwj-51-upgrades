using Godot;

namespace Bread
{
    public class PausePopup : PopupPanel
    {
        public override void _Ready()
        {
            GetNode("VBoxContainer/HBoxContainer/Return").Connect("pressed", this, nameof(Return));
            GetNode("VBoxContainer/HBoxContainer/Quit").Connect("pressed", this, nameof(QuitGame));
        }

        public override void _Input(InputEvent evt)
        {
            if (evt.IsActionReleased("pause"))
                SetPaused(!GetTree().Paused);
        }

        private void QuitGame()
        {
            Return();

            GetTree().ChangeScene("res://scenes/menus/MainMenu.tscn");
        }

        private void Return()
        {
            SetPaused(false);
        }

        private void SetPaused(bool pause)
        {
            if (pause)
            {
                // Pause
                PopupCentered(new Vector2(0, 0));
                GetTree().Paused = true;
            }
            else
            {
                // Resume
                Hide();
                GetTree().Paused = false;
            }
        }

    }
}