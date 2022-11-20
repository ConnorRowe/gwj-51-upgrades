using Godot;
using System;

namespace Bread
{
    public class EndScreen : Control
    {
        // Declare member variables here. Examples:
        // private int a = 2;
        // private string b = "text";

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            GetNode("Button").Connect("pressed", this, nameof(ToMainMenu));
        }

        void ToMainMenu()
        {
            GetTree().ChangeScene("res://scenes/menus/MainMenu.tscn");
            QueueFree();
        }

        //  // Called every frame. 'delta' is the elapsed time since the previous frame.
        //  public override void _Process(float delta)
        //  {
        //
        //  }
    }
}