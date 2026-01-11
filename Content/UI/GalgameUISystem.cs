using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using zhashi.Content.UI.Profiles;
using zhashi.Content.Items; // 引用阿罗德斯物品ID

namespace zhashi.Content.UI
{
    [Autoload(Side = ModSide.Client)]
    public class GalgameUISystem : ModSystem
    {
        internal UserInterface galgameInterface;
        internal GalgameDialogueUI galgameUI;
        public static bool visible = false;

        // 标记当前是否正在和阿罗德斯对话
        public static bool isTalkingToArrodes = false;

        public override void Load()
        {
            galgameUI = new GalgameDialogueUI();
            galgameUI.Activate();
            galgameInterface = new UserInterface();
            galgameInterface.SetState(galgameUI);
        }

        public static void OpenArrodesUI()
        {
            if (visible && isTalkingToArrodes)
            {
                visible = false;
                isTalkingToArrodes = false;
            }
            else
            {
                visible = true;
                isTalkingToArrodes = true;
                Main.npcChatText = "";
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            // 这里只负责处理点击等逻辑更新
            // 显示/隐藏的核心判断移交给了 CheckIfShouldShowUI
            bool shouldShow = CheckIfShouldShowUI();

            if (shouldShow)
            {
                if (!visible) OpenUI();
                galgameInterface?.Update(gameTime);
            }
            else
            {
                if (visible) CloseUI();
            }
        }

        // ★★★ 核心方法：实时判断是否应该显示自定义 UI ★★★
        private bool CheckIfShouldShowUI()
        {
            // 1. 阿罗德斯模式
            if (isTalkingToArrodes)
            {
                // 如果移动、背包开启等，则不显示
                if (Main.LocalPlayer.controlLeft || Main.LocalPlayer.controlRight ||
                    Main.LocalPlayer.controlDown || Main.LocalPlayer.controlUp ||
                    Main.playerInventory)
                {
                    return false;
                }
                return true;
            }

            // 2. 普通 NPC 模式
            int talkNPC = Main.LocalPlayer.talkNPC;
            if (talkNPC != -1)
            {
                // 如果打开了制作菜单(向导) 或者 打开了商店，则不显示自定义 UI (显示原版)
                if (Main.InGuideCraftMenu || Main.npcShop != 0)
                {
                    return false;
                }

                int npcType = Main.npc[talkNPC].type;

                // ★ 白名单检查 ★
                // 只有这些 NPC 才拦截原版 UI，显示我们的 UI
                if (npcType == NPCID.Nurse ||
                    npcType == NPCID.Guide ||
                    npcType == NPCID.Dryad ||
                    npcType == NPCID.Angler)
                {
                    return true;
                }
            }

            return false;
        }

        public void OpenUI()
        {
            visible = true;
            Main.npcChatText = "";
        }

        public void CloseUI()
        {
            visible = false;
            isTalkingToArrodes = false;
        }

        // ★★★ 消除闪烁的关键：在绘制层直接拦截 ★★★
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int dialogIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: NPC / Sign Dialog"));

            if (dialogIndex != -1)
            {
                // 这里不依赖 'visible' 变量，而是现场直接判断
                // 这样即使 UpdateUI 慢了一帧，这里也能在绘制前把原版 UI 删掉
                if (CheckIfShouldShowUI())
                {
                    // 1. 移除原版对话框层 (让原版 UI 彻底消失)
                    layers.RemoveAt(dialogIndex);

                    // 2. 插入我们的 UI 层
                    layers.Insert(dialogIndex, new LegacyGameInterfaceLayer(
                        "zhashi: GalgameUI",
                        delegate
                        {
                            // 强制同步 visible 状态，防止逻辑层没跟上
                            visible = true;
                            galgameInterface.Draw(Main.spriteBatch, new GameTime());
                            return true;
                        },
                        InterfaceScaleType.UI)
                    );
                }
            }
        }
    }
}