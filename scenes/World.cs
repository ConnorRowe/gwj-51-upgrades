using Godot;
using System.Collections.Generic;

namespace Bread
{
    public class World : Node2D
    {
        public enum DeathType
        {
            RESET,
            WATER
        }

        static Dictionary<DeathType, string[]> deathMessages = new Dictionary<DeathType, string[]>()
        {
            {DeathType.RESET, new string[3] { "got stuck?", "back to where you were", "struggling..?" }},
            {DeathType.WATER, new string[3] { "don't swim with the fishes", "water is wet", "bit of rain never killed anyone" }}
        };

        static World INSTANCE;
        static PackedScene playerScene = GD.Load<PackedScene>("res://scenes/Player.tscn");
        static PackedScene smokePuffScene = GD.Load<PackedScene>("res://scenes/SmokePuff.tscn");
        static PackedScene bigCrumbsScene = GD.Load<PackedScene>("res://scenes/BigCrumbs.tscn");
        static RandomNumberGenerator rng = new RandomNumberGenerator();
        static World()
        {
            rng.Randomize();
        }

        public static Player Player { get; private set; } = null;
        public static UpgradePopup UpgradePopup { get; private set; }
        Checkpoint lastCheckpoint;
        PlayerBody targetPlayerBody = null;
        Camera2D camera;
        SceneTreeTween cameraTween;
        RoomArea currentRoom = null;
        bool movingCamera = false;
        TextureRect bg;
        ShaderMaterial bgMat;
        Shaker deathMSGShaker;
        Label deathMSG;
        SceneTreeTween deathTween = null;
        float count = 0;

        public override void _Ready()
        {
            INSTANCE = this;

            camera = GetNode<Camera2D>("Camera2D");
            bg = GetNode<TextureRect>("Background/BG");
            bgMat = (ShaderMaterial)bg.Material;
            deathMSG = GetNode<Label>("UI/DeathMSGHolder/DeathMSG");
            deathMSGShaker = GetNode<Shaker>("UI/DeathMSGHolder/Shaker");

            MoveCameraToRoom(GetNode<RoomArea>("RoomAreas/IntroRoom"), instant: true);

            if (SaveData.LastCheckpoint.Length > 0 && GetNodeOrNull(SaveData.LastCheckpoint) is Checkpoint checkpoint)
            {
                CheckpointActivated(checkpoint);
                SpawnPlayer(checkpoint.GlobalPosition);
            }

            UpgradePopup = GetNode<UpgradePopup>("UI/UpgradePopup");

            GameStageChanged(SaveData.GameStage);
        }

        public override void _PhysicsProcess(float delta)
        {
            if (!movingCamera && targetPlayerBody != null)
                camera.GlobalPosition = targetPlayerBody.GlobalPosition;
        }

        public override void _Process(float delta)
        {
            count += delta * 4f;
            if (count > Mathf.Tau)
                count -= Mathf.Tau;

            deathMSG.RectRotation = Mathf.Cos(count) * 10f;
        }

        public override void _Input(InputEvent evt)
        {
            if (evt.IsActionReleased("reset"))
            {
                KillPlayer(DeathType.RESET);
            }
        }

        public static void GameStageChanged(int gameStage)
        {
            if (gameStage >= 2)
                INSTANCE.GetNode<Sprite>("UI/Upgrades/PB").Visible = true;
            if (gameStage >= 3)
                INSTANCE.GetNode<Sprite>("UI/Upgrades/SD").Visible = true;

        }

        public static void MoveCameraToRoom(RoomArea roomArea, bool instant = false)
        {
            if (INSTANCE.currentRoom != null && INSTANCE.currentRoom.Name == roomArea.Name)
                return;

            GD.Print("MoveCameraToRoom ", roomArea, " instant:", instant);

            INSTANCE.currentRoom = roomArea;

            if (INSTANCE.cameraTween != null)
            {
                INSTANCE.cameraTween.Kill();
                INSTANCE.cameraTween.Dispose();
            }

            if (instant)
            {
                INSTANCE.camera.GlobalPosition = roomArea.RoomPosition;
                INSTANCE.camera.LimitLeft = roomArea.LimitLeft;
                INSTANCE.camera.LimitTop = roomArea.LimitTop;
                INSTANCE.camera.LimitRight = roomArea.LimitRight;
                INSTANCE.camera.LimitBottom = roomArea.LimitBottom;

                INSTANCE.bgMat.SetShaderParam("transition_amount", 0f);
                INSTANCE.bgMat.SetShaderParam("bg_1", roomArea.BackgroundTexture);

                return;
            }

            INSTANCE.cameraTween = INSTANCE.CreateTween();
            INSTANCE.cameraTween.SetParallel();
            INSTANCE.cameraTween.SetEase(Tween.EaseType.Out);

            //INSTANCE.movingCamera = true;

            //var translateTween = INSTANCE.cameraTween.TweenProperty(INSTANCE.camera, "global_position", roomArea.RoomPosition, .6f);
            //translateTween.SetTrans(Tween.TransitionType.Cubic);
            //translateTween.FromCurrent();

            INSTANCE.camera.LimitLeft = Mathf.Min(INSTANCE.camera.LimitLeft, roomArea.LimitLeft);
            INSTANCE.camera.LimitTop = Mathf.Min(INSTANCE.camera.LimitTop, roomArea.LimitTop);
            INSTANCE.camera.LimitRight = Mathf.Max(INSTANCE.camera.LimitRight, roomArea.LimitRight);
            INSTANCE.camera.LimitBottom = Mathf.Max(INSTANCE.camera.LimitBottom, roomArea.LimitBottom);

            var leftTween = INSTANCE.cameraTween.TweenProperty(INSTANCE.camera, "limit_left", roomArea.LimitLeft, .3f);
            var topTween = INSTANCE.cameraTween.TweenProperty(INSTANCE.camera, "limit_top", roomArea.LimitTop, .3f);
            var rightTween = INSTANCE.cameraTween.TweenProperty(INSTANCE.camera, "limit_right", roomArea.LimitRight, .3f);
            var bottomTween = INSTANCE.cameraTween.TweenProperty(INSTANCE.camera, "limit_bottom", roomArea.LimitBottom, .3f);

            string next_bg_prop = "";
            float next_bg_amount = 0f;
            if ((float)INSTANCE.bgMat.GetShaderParam("transition_amount") > .5f)
            {
                next_bg_prop = "bg_1";
                next_bg_amount = 0f;
            }
            else
            {
                next_bg_prop = "bg_2";
                next_bg_amount = 1f;
            }

            INSTANCE.bgMat.SetShaderParam(next_bg_prop, roomArea.BackgroundTexture);

            var transitionTween = INSTANCE.cameraTween.TweenProperty(INSTANCE.bgMat, "shader_param/transition_amount", next_bg_amount, .6f);
            transitionTween.FromCurrent();

            foreach (var limitTween in new PropertyTweener[] { leftTween, topTween, rightTween, bottomTween })
            {
                limitTween.SetDelay(.6f);
                limitTween.FromCurrent();
            }

            INSTANCE.cameraTween.Connect("finished", INSTANCE, nameof(ResetMovingCamera));
        }

        private void ResetMovingCamera()
        {
            movingCamera = false;
        }

        public static void SpawnPlayer(Vector2 globalPosition)
        {
            if (Player != null)
            {
                INSTANCE.RemoveChild(Player);
                Player.QueueFree();
            }

            Player = playerScene.Instance<Player>();
            INSTANCE.AddChild(Player);
            INSTANCE.targetPlayerBody = Player.GetNode<PlayerBody>("Middle3");
            Player.GlobalPosition = globalPosition;
        }

        public static void CheckpointActivated(Checkpoint checkpoint)
        {
            INSTANCE.lastCheckpoint = checkpoint;
            SaveData.LastCheckpoint = INSTANCE.GetPathTo(checkpoint);
            SaveData.SaveToDisk();
        }

        public static void MakeSmokePuff(Vector2 globalPosition)
        {
            var smokePuff = smokePuffScene.Instance<Particles2D>();
            INSTANCE.AddChild(smokePuff);
            smokePuff.GlobalPosition = globalPosition;
            smokePuff.Emitting = true;
            var tmr = INSTANCE.GetTree().CreateTimer(smokePuff.Lifetime);
            tmr.Connect("timeout", smokePuff, "queue_free");
        }

        public static void MakeBigCrumbs(Vector2 globalPosition)
        {
            var bigCrumbs = bigCrumbsScene.Instance<Particles2D>();
            INSTANCE.AddChild(bigCrumbs);
            bigCrumbs.GlobalPosition = globalPosition;
            bigCrumbs.Emitting = true;
            var tmr = INSTANCE.GetTree().CreateTimer(bigCrumbs.Lifetime);
            tmr.Connect("timeout", bigCrumbs, "queue_free");
        }

        public static void KillPlayer(DeathType deathType)
        {
            INSTANCE.ResetPlayerToCheckpoint();
            Sounds.Die();
            INSTANCE.DeathMSG(deathType);
        }

        void ResetPlayerToCheckpoint()
        {
            if (lastCheckpoint != null)
                SpawnPlayer(lastCheckpoint.GlobalPosition);
            else if ((currentRoom is IntroRoom ir && !currentRoom.GetNode<AnimationPlayer>("AnimationPlayer").IsPlaying()))
                SpawnPlayer(new Vector2(160, 90));
        }

        void DeathMSG(DeathType deathType)
        {
            deathMSG.Text = deathMessages[deathType][rng.RandiRange(0, deathMessages[deathType].Length - 1)];

            if (deathTween != null)
            {
                deathTween.Kill();
                deathTween.Dispose();
            }

            deathTween = CreateTween().SetParallel();

            deathTween.SetEase(Tween.EaseType.In);
            deathTween.SetTrans(Tween.TransitionType.Cubic);

            var deathScale = deathTween.TweenProperty(deathMSG, "rect_scale", Vector2.One, 1);
            deathScale.From(Vector2.Zero);

            deathTween.Chain().TweenInterval(2.5f);

            var deathScaleBack = deathTween.Chain().TweenProperty(deathMSG, "rect_scale", Vector2.Zero, 1);
            deathScaleBack.From(Vector2.One);

            deathMSGShaker.Shake(1);
        }
    }
}