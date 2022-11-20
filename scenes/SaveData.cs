using Godot;

namespace Bread
{
    public static class SaveData
    {
        private const string filePath = "user://save.ini";
        private static ConfigFile Save { get; } = new ConfigFile();

        public static float MasterVol
        {
            get { return (float)Save.GetValue("Settings", "master_vol"); }
            set { SetValue("master_vol", value); }
        }
        public static float MusicVol
        {
            get { return (float)Save.GetValue("Settings", "music_vol"); }
            set { SetValue("music_vol", value); }
        }
        public static float SFXVol
        {
            get { return (float)Save.GetValue("Settings", "sfx_vol"); }
            set { SetValue("sfx_vol", value); }
        }
        public static bool Fullscreen
        {
            get { return (bool)Save.GetValue("Settings", "fullscreen"); }
            set { SetValue("fullscreen", value); }
        }
        public static string LastCheckpoint
        {
            get { return (string)Save.GetValue("Settings", "last_checkpoint"); }
            set { SetValue("last_checkpoint", value); }
        }
        public static int GameStage
        {
            get { return (int)Save.GetValue("Settings", "game_stage"); }
            set { SetValue("game_stage", value); World.GameStageChanged(value); }
        }

        static SaveData()
        {
            var e = Save.Load(filePath);

            if (e == Error.Ok && Save.HasSection("Settings"))
            {
                GD.Print("Loaded save data.");
            }
            else
            {
                GD.Print("No save file found. Re-creating.", e);
                Save.SetValue("Settings", "master_vol", .8f);
                Save.SetValue("Settings", "music_vol", .5f);
                Save.SetValue("Settings", "sfx_vol", .5f);
                Save.SetValue("Settings", "fullscreen", false);
                Save.SetValue("Settings", "last_checkpoint", "");
                Save.SetValue("Settings", "game_stage", 0);
                Save.Save(filePath);
                Save.Load(filePath);
            }
        }

        public static void SetValue(string key, object value) => Save.SetValue("Settings", key, value);
        public static void SaveToDisk() => Save.Save(filePath);

        public static void ApplyLoadedSettings()
        {
            AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), GD.Linear2Db(MasterVol));
            AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), GD.Linear2Db(MusicVol));
            AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("SFX"), GD.Linear2Db(SFXVol));
            OS.WindowFullscreen = Fullscreen;
        }
    }
}