using Godot;

namespace Bread
{
    public class Sounds : Node
    {
        static AudioStreamPlayer blipPlayer;
        static AudioStreamPlayer softSlapPlayer;
        static AudioStreamPlayer medSlapPlayer;
        static AudioStreamPlayer hardSlapPlayer;
        public override void _Ready()
        {
            base._Ready();

            blipPlayer = GetNode<AudioStreamPlayer>("Blip");
            softSlapPlayer = GetNode<AudioStreamPlayer>("SoftSlap");
            medSlapPlayer = GetNode<AudioStreamPlayer>("MedSlap");
            hardSlapPlayer = GetNode<AudioStreamPlayer>("HardSlap");
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
    }
}