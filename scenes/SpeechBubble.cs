using Godot;
using System;

namespace Bread
{
    public class SpeechBubble : Node2D
    {
        SceneTreeTween tween;
        public Vector2 Offset { get; set; } = Vector2.Zero;
        public Node2D FollowNode { get; set; }

        public override void _Ready()
        {
            tween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Bounce);

            var scale = tween.TweenProperty(this, "scale", Vector2.One, .3f);
            scale.From(Vector2.Zero);
            var textReveal = tween.Parallel().TweenProperty(GetNode("Container/MarginContainer/Label"), "percent_visible", 1f, 1f);
            textReveal.From(0f);
            textReveal.SetTrans(Tween.TransitionType.Linear);

            tween.TweenInterval(5f);

            var hide = tween.TweenProperty(this, "scale", Vector2.Zero, .2f);
            hide.From(Vector2.One);

            tween.Connect("finished", this, "queue_free");
        }

        public override void _PhysicsProcess(float delta)
        {
            if (IsInstanceValid(FollowNode))
                GlobalPosition = FollowNode.GlobalPosition + Offset;
            else
            {
                tween.Kill();
                QueueFree();
            }
        }

        public void SetText(string text)
        {
            GetNode<Label>("Container/MarginContainer/Label").Text = text;
        }
    }
}