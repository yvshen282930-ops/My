using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using zhashi.Content; // 引用 Player 类

namespace zhashi.Content.Items
{
    public abstract class LotMItem : ModItem
    {
        // 物品所属途径 (Hunter, Fool, Giant, Moon)
        public virtual string Pathway => "None";

        // 需要的序列等级 (9, 8, 7...)
        public virtual int RequiredSequence => 0;

        // 【核心修改1】统一检查能否使用
        public override bool CanUseItem(Player player)
        {
            if (Pathway != "None" && RequiredSequence > 0)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

                // 检查是否满足条件
                if (!CheckSequenceRequirement(modPlayer))
                {
                    return false; // 不满足则不能使用
                }
            }
            return true;
        }

        // 【核心修改2】统一添加提示栏
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (RequiredSequence > 0)
            {
                Player player = Main.LocalPlayer;
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

                string colorHex = "FF0000"; // 默认红色 (未满足)
                string statusText = "未满足";

                // 检查条件
                if (CheckSequenceRequirement(modPlayer))
                {
                    colorHex = "00FF00"; // 绿色 (已满足)
                    statusText = "已满足";
                }

                string text = $"[c/{colorHex}:需要 {Pathway} 途径 序列 {RequiredSequence} ({statusText})]";
                tooltips.Add(new TooltipLine(Mod, "SequenceReq", text));
            }
        }

        // --- 辅助方法：检查玩家是否满足序列要求 ---
        private bool CheckSequenceRequirement(LotMPlayer p)
        {
            // 1. 猎人途径
            if (Pathway == "Hunter")
            {
                return p.currentHunterSequence <= RequiredSequence;
            }
            // 2. 愚者途径 (您之前缺少的)
            else if (Pathway == "Fool")
            {
                return p.currentFoolSequence <= RequiredSequence;
            }
            // 3. 巨人途径
            else if (Pathway == "Giant")
            {
                return p.currentSequence <= RequiredSequence;
            }
            // 4. 月亮途径
            else if (Pathway == "Moon")
            {
                return p.currentMoonSequence <= RequiredSequence;
            }

            return false;
        }
    }
}