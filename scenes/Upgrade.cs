using Godot;
using System;

namespace Bread
{
    public class Upgrade : Area2D
    {
        [Export]
        public int GameStage { get; set; } = 0;
        [Export]
        public string UpgradeName { get; set; }
        [Export]
        public string UpgradeDesc { get; set; }
        public override void _Ready()
        {
            if (SaveData.GameStage >= GameStage)
            {
                QueueFree();
            }

            Connect("body_entered", this, nameof(BodyEntered));
        }

        void BodyEntered(Node body)
        {
            if (body is PlayerBody pb)
            {
                QueueFree();

                World.UpgradePopup.PopUp(UpgradeName, UpgradeDesc);
                Sounds.Upgrade();

                if(GameStage == 2)
                    pb.GetParent<Player>().PeanutButterUpgrade();
                else if(GameStage == 3)
                    pb.GetParent<Player>().SourdoughUpgrade();
                if (SaveData.GameStage < GameStage)
                {
                    SaveData.GameStage = GameStage;
                    SaveData.SaveToDisk();
                }
            }
        }
    }
}