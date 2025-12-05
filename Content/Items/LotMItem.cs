using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using zhashi.Content; // 确保引用你的命名空间

namespace zhashi.Content.Items
{
    // 这是一个抽象基类，它不能直接变成物品，而是给其他物品继承用的
    public abstract class LotMItem : ModItem
    {
        // 【自定义属性】
        // 这个物品属于哪条途径？默认为 "None" (无)
        public virtual string Pathway => "None";

        // 这个物品需要的序列等级？0 代表不需要等级
        // 假设逻辑：数字越小越强（序列9 -> 序列1）
        public virtual int RequiredSequence => 0;

        // 【核心功能：统一检查能否使用】
        public override bool CanUseItem(Player player)
        {
            // 如果定义了途径和序列要求
            if (Pathway != "None" && RequiredSequence > 0)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

                // 检查逻辑：如果是猎人途径物品，且玩家当前序列数值 > 要求数值（比如玩家序列9 > 要求序列1），则不可用
                if (Pathway == "Hunter" && modPlayer.currentHunterSequence > RequiredSequence)
                {
                    return false; // 序列不够，禁止使用
                }
            }
            // 如果通过了检查，或者没有要求，则允许使用
            return true;
        }

        // 【核心功能：统一添加提示栏】
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (RequiredSequence > 0)
            {
                Player player = Main.LocalPlayer;
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

                string colorHex = "FF0000"; // 默认红色（未满足）
                string statusText = "未满足";

                // 只有当途径匹配 且 序列足够时，才显示绿色
                // 这里假设你只做了猎人途径，以后可以加 || Pathway == "Seer" 等
                if (Pathway == "Hunter" && modPlayer.currentHunterSequence <= RequiredSequence)
                {
                    colorHex = "00FF00"; // 绿色（已满足）
                    statusText = "已满足";
                }

                // 添加一行文本到介绍的最下面
                string text = $"[c/{colorHex}:需要 {Pathway} 途径 序列 {RequiredSequence} ({statusText})]";
                tooltips.Add(new TooltipLine(Mod, "SequenceReq", text));
            }
        }
    }
}