using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace zhashi.Content.UI
{
    [Autoload(true)]
    public class SequenceUISystem : ModSystem
    {
        internal SequenceStatusUI sequenceState;
        private UserInterface _sequenceInterface;

        public override void Load()
        {
            // 仅在客户端加载 UI
            if (!Main.dedServ)
            {
                sequenceState = new SequenceStatusUI();
                sequenceState.Activate();
                _sequenceInterface = new UserInterface();
                _sequenceInterface.SetState(sequenceState);
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (_sequenceInterface != null)
            {
                _sequenceInterface.Update(gameTime);
            }
        }

        // 将 UI 插入到游戏绘制层中
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            // 寻找资源条 (Resource Bars) 图层索引
            int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));

            if (resourceBarIndex != -1)
            {
                layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer(
                    "zhashi: Sequence Interface",
                    delegate {
                        // 当不在主菜单且UI存在时绘制
                        if (_sequenceInterface != null && _sequenceInterface.CurrentState != null)
                        {
                            _sequenceInterface.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}