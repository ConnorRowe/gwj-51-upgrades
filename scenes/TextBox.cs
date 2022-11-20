using Godot;

namespace Bread
{
    public class TextBox : NinePatchRect
    {
        [Export]
        public Color BoxColour { get; set; } = Colors.White;

        RichTextLabel richTextLabel;
        public string BBCode
        {
            get { return richTextLabel.BbcodeText; }
            set
            {
                if (BBCode != value)
                {
                    richTextLabel.BbcodeText = value;
                    richTextLabel.VisibleCharacters = 0;
                    richTextLabel.PercentVisible = 0;
                }

            }
        }

        float nextCharSpeed = .05f;
        float count = 0f;

        public override void _Ready()
        {
            richTextLabel = GetNode<RichTextLabel>("RichTextLabel");

            SelfModulate = BoxColour;
        }

        public override void _Process(float delta)
        {
            if (richTextLabel.PercentVisible < 1f)
            {
                count += delta;
                if (count >= nextCharSpeed)
                {
                    int chars = Mathf.CeilToInt(count / nextCharSpeed);
                    count -= nextCharSpeed;
                    richTextLabel.VisibleCharacters += chars;
                    Sounds.Blip();
                }
            }
        }
    }
}