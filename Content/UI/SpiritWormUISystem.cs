using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace zhashi.Content.UI
{
    [Autoload(Side = ModSide.Client)] // 只在客户端运行
    public class SpiritWormUISystem : ModSystem
    {
        internal SpiritWormBar spiritWormBar;
        private UserInterface spiritWormInterface;

        // 加载模组时初始化 UI
        public override void Load()
        {
            spiritWormBar = new SpiritWormBar();
            spiritWormBar.Activate();
            spiritWormInterface = new UserInterface();
            spiritWormInterface.SetState(spiritWormBar);
        }

        // 每一帧更新 UI 逻辑
        public override void UpdateUI(GameTime gameTime)
        {
            spiritWormInterface?.Update(gameTime);
        }

        // 核心：将 UI 插入游戏绘制层
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            // 找到资源条 (血条/蓝条) 的绘制层索引
            int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));

            if (resourceBarIndex != -1)
            {
                // 在资源条之后绘制我们的 UI
                layers.Insert(resourceBarIndex + 1, new LegacyGameInterfaceLayer(
                    "zhashi: Spirit Worm Bar",
                    delegate {
                        spiritWormInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}