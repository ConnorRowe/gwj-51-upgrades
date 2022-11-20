using Godot;
using System;

namespace Bread
{
    public class UpgradePopup : Control
    {
        RichTextLabel title;
        RichTextLabel body;
        SceneTreeTween tween;

        public override void _Ready()
        {
            title = GetNode<RichTextLabel>("Title");
            body = GetNode<RichTextLabel>("Body");

            title.RectScale = body.RectScale = Vector2.Zero;
        }

        public void PopUp(string upgradeName, string upgradeDesc)
        {
            body.BbcodeText = string.Format("[center][color=#fef3c0]{0}:\n\n{1}", upgradeName, upgradeDesc);

            if (tween != null)
            {
                tween.Kill();
                tween.Dispose();
            }

            tween = CreateTween().SetParallel(true);
            tween.SetEase(Tween.EaseType.In);
            tween.SetTrans(Tween.TransitionType.Bounce);

            var titleScale = tween.TweenProperty(title, "rect_scale", Vector2.One, 1);
            var bodyScale = tween.TweenProperty(body, "rect_scale", Vector2.One, 1);
            titleScale.From(Vector2.Zero);
            bodyScale.From(Vector2.Zero);

            tween.Chain().TweenInterval(5);

            var titleScaleBack = tween.Chain().TweenProperty(title, "rect_scale", Vector2.Zero, 1);
            var bodyScaleBack = tween.Chain().TweenProperty(body, "rect_scale", Vector2.Zero, 1);
            titleScaleBack.From(Vector2.One);
            bodyScaleBack.From(Vector2.One);
        }
    }
}