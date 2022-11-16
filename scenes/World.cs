using Godot;

namespace Bread
{
    public class World : Node2D
    {
        static World INSTANCE;

        PlayerBody targetPlayerBody;
        Camera2D camera;
        SceneTreeTween cameraTween;
        RoomArea currentRoom;
        bool movingCamera = false;

        public override void _Ready()
        {
            INSTANCE = this;

            camera = GetNode<Camera2D>("Camera2D");
            targetPlayerBody = GetNode<PlayerBody>("Player/Middle3");

            MoveCameraToRoom(GetNode<RoomArea>("RoomAreas/FirstRoom"), instant: true);
        }

        public override void _PhysicsProcess(float delta)
        {
            if (!movingCamera)
                camera.GlobalPosition = targetPlayerBody.GlobalPosition;
        }

        public static void MoveCameraToRoom(RoomArea roomArea, bool instant = false)
        {
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

                return;
            }

            INSTANCE.cameraTween = INSTANCE.CreateTween();

            INSTANCE.movingCamera = true;

            var translateTween = INSTANCE.cameraTween.TweenProperty(INSTANCE.camera, "global_position", roomArea.RoomPosition, .6f);
            translateTween.SetTrans(Tween.TransitionType.Cubic);
            translateTween.SetEase(Tween.EaseType.InOut);

            INSTANCE.camera.LimitLeft = Mathf.Min(INSTANCE.camera.LimitLeft, roomArea.LimitLeft);
            INSTANCE.camera.LimitTop = Mathf.Min(INSTANCE.camera.LimitTop, roomArea.LimitTop);
            INSTANCE.camera.LimitRight = Mathf.Max(INSTANCE.camera.LimitRight, roomArea.LimitRight);
            INSTANCE.camera.LimitBottom = Mathf.Max(INSTANCE.camera.LimitBottom, roomArea.LimitBottom);

            var leftTween = INSTANCE.cameraTween.TweenProperty(INSTANCE.camera, "limit_left", roomArea.LimitLeft, .3f);
            var topTween = INSTANCE.cameraTween.TweenProperty(INSTANCE.camera, "limit_top", roomArea.LimitTop, .3f);
            var rightTween = INSTANCE.cameraTween.TweenProperty(INSTANCE.camera, "limit_right", roomArea.LimitRight, .3f);
            var bottomTween = INSTANCE.cameraTween.TweenProperty(INSTANCE.camera, "limit_bottom", roomArea.LimitBottom, .3f);

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
    }
}