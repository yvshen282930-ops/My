using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace zhashi.Content.UI
{
    public class DogUISystem : ModSystem
    {
        internal DogStatsUI dogStatsUI;
        private UserInterface dogInterface;

        public override void Load()
        {
            // 游戏加载时初始化 UI
            dogStatsUI = new DogStatsUI();
            dogStatsUI.Activate();
            dogInterface = new UserInterface();
            dogInterface.SetState(dogStatsUI);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (DogStatsUI.Visible)
            {
                dogInterface?.Update(gameTime);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "zhashi: DogStatsUI",
                    delegate
                    {
                        if (DogStatsUI.Visible)
                        {
                            dogInterface.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}