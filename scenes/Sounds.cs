using Godot;

namespace Bread
{
    public class Sounds : Node
    {
        static AudioStreamPlayer blipPlayer;
        static AudioStreamPlayer softSlapPlayer;
        static AudioStreamPlayer medSlapPlayer;
        static AudioStreamPlayer hardSlapPlayer;
        static AudioStreamPlayer musicPlayer;
        static AudioStreamPlayer upgradePlayer;
        static AudioStreamPlayer checkpointPlayer;
        static AudioStreamPlayer diePlayer;
        public override void _Ready()
        {
            base._Ready();

            blipPlayer = GetNode<AudioStreamPlayer>("Blip");
            softSlapPlayer = GetNode<AudioStreamPlayer>("SoftSlap");
            medSlapPlayer = GetNode<AudioStreamPlayer>("MedSlap");
            hardSlapPlayer = GetNode<AudioStreamPlayer>("HardSlap");
            musicPlayer = GetNode<AudioStreamPlayer>("MusicPlayer");
            upgradePlayer = GetNode<AudioStreamPlayer>("UpgradePlayer");
            checkpointPlayer = GetNode<AudioStreamPlayer>("CheckpointPlayer");
            diePlayer = GetNode<AudioStreamPlayer>("DiePlayer");
        }

        public static void Blip()
        {
            blipPlayer.Play();
        }
        public static void SoftSlap(float vol)
        {
            if (!softSlapPlayer.Playing)
            {
                softSlapPlayer.VolumeDb = GD.Linear2Db(vol);
                softSlapPlayer.Play();
            }
        }
        public static void MedSlap()
        {
            if (!medSlapPlayer.Playing)
                medSlapPlayer.Play();
        }
        public static void HardSlap()
        {
            if (!hardSlapPlayer.Playing)
                hardSlapPlayer.Play();
        }

        public static void PlayMusic(AudioStreamOGGVorbis track)
        {
            if (musicPlayer.Stream != track)
            {
                musicPlayer.Stream = track;
                musicPlayer.Play();
            }
        }

        public static void Upgrade()
        {
            if (!upgradePlayer.Playing)
                upgradePlayer.Play();
        }
        public static void Checkpoint()
        {
            if (!checkpointPlayer.Playing)
                checkpointPlayer.Play();
        }
        public static void Die()
        {
            if (!diePlayer.Playing)
                diePlayer.Play();
        }
    }
}