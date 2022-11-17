using Godot;

namespace Bread
{
    public class IntroRoom : RoomArea
    {
        AnimationPlayer animationPlayer;
        AnimatedSprite pressSpaceToCont;
        TextBox workerTextBox;
        TextBox bossTextBox;

        int animNum = 0;
        static string[] animationNames = new string[7] { "start", "sigh", "thunder", "shutup", "back_to_work", "lightning_strike", "bread_is_born" };

        public override void _Ready()
        {
            base._Ready();

            pressSpaceToCont = GetNode<AnimatedSprite>("CanvasLayer/PressSpaceToCont");
            workerTextBox = GetNode<TextBox>("CanvasLayer/WorkerTextBox");
            bossTextBox = GetNode<TextBox>("CanvasLayer/BossTextBox");

            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            animationPlayer.Connect("animation_started", this, nameof(AnimationStarted));
            animationPlayer.Connect("animation_finished", this, nameof(AnimationFinished));
        }

        public override void _Input(InputEvent evt)
        {
            base._Input(evt);

            if (evt.IsActionPressed("next") && pressSpaceToCont.Visible)
            {
                NextAnim();
            }
        }

        public void AnimationStarted(string animation)
        {
            pressSpaceToCont.Visible = false;
            workerTextBox.Visible = false;
            bossTextBox.Visible = false;
        }

        public void AnimationFinished(string animation)
        {
            pressSpaceToCont.Visible = true;
        }

        public void StopConveyor()
        {
            GetNode<AnimationPlayer>("ConveyorPlayer").GetAnimation("Convey").Loop = false;
        }

        public void SetWorkerText(string text)
        {
            workerTextBox.Visible = true;
            workerTextBox.BBCode = text;
        }

        public void SetBossText(string text)
        {
            bossTextBox.Visible = true;
            bossTextBox.BBCode = text;
        }

        private void NextAnim()
        {
            animNum++;
            if(animNum < animationNames.Length)
                animationPlayer.Play(animationNames[animNum]);
        }

        private void SpawnPlayer()
        {
            World.SpawnPlayer(GetNode<Position2D>("PlayerSpawn").GlobalPosition);
        }

    }
}