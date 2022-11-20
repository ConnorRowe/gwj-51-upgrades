using Godot;
using System;

namespace Bread
{
    public class MainMenu : Control
    {
        static PackedScene worldScene = GD.Load<PackedScene>("res://scenes/World.tscn");
        static AudioStreamOGGVorbis mainmenuMusic = GD.Load<AudioStreamOGGVorbis>("res://audio/music/mainmenu.ogg");

        VBoxContainer settings;
        VBoxContainer main;

        public override void _EnterTree()
        {
            SaveData.ApplyLoadedSettings();
        }

        public override void _Ready()
        {
            GetNode("Main/Continue").Connect("pressed", this, nameof(Play));
            GetNode("Main/NewGame").Connect("pressed", this, nameof(NewGame));
            GetNode("Main/Settings").Connect("pressed", this, nameof(ShowSettings));
            GetNode("Settings/Back").Connect("pressed", this, nameof(ShowMain));

            settings = GetNode<VBoxContainer>("Settings");
            main = GetNode<VBoxContainer>("Main");

            Sounds.PlayMusic(mainmenuMusic);

            if (SaveData.GameStage > 0)
                GetNode<BaseButton>("Main/Continue").Disabled = false;

        }

        private void NewGame()
        {
            SaveData.GameStage = 0;
            SaveData.LastCheckpoint = "";
            SaveData.SaveToDisk();

            Play();
        }

        private void Play()
        {
            Sounds.PlayMusic(null);
            GetTree().ChangeSceneTo(worldScene);
            QueueFree();
        }

        private void ShowSettings()
        {
            settings.Visible = true;
            main.Visible = false;
        }

        private void ShowMain()
        {
            settings.Visible = false;
            main.Visible = true;
        }
    }
}