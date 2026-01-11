using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content; // 引用 Player 类

namespace zhashi.Content.Items
{
    public abstract class LotMItem : ModItem
    {
        // ==========================================
        // 1. 基础属性设置
        // ==========================================

        // 物品所属途径 (Hunter, Fool, Giant, Moon, Marauder, Sun)
        public virtual string Pathway => "None";

        // 需要的序列等级 (9, 8, 7...)，0 表示无限制
        public virtual int RequiredSequence => 0;

        // ==========================================
        // 2. 核心功能：使用限制检查
        // ==========================================
        public override bool CanUseItem(Player player)
        {
            // 如果设定了途径且有序列要求
            if (Pathway != "None" && RequiredSequence > 0)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

                // 检查是否满足条件
                if (!CheckSequenceRequirement(modPlayer))
                {
                    return false; // 不满足则禁止使用
                }
            }
            return true;
        }

        // ==========================================
        // 3. 核心功能：智能提示栏 (Tooltip)
        // ==========================================
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (RequiredSequence > 0 && Pathway != "None")
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

                // 汉化途径名称
                string pathName = Pathway;
                if (Pathway == "Fool") pathName = "愚者";
                else if (Pathway == "Giant") pathName = "巨人/战士";
                else if (Pathway == "Hunter") pathName = "猎人";
                else if (Pathway == "Moon") pathName = "月亮";
                else if (Pathway == "Marauder") pathName = "错误";
                else if (Pathway == "Sun") pathName = "太阳";

                string text = $"[c/{colorHex}:需要 {pathName} 途径 序列 {RequiredSequence} ({statusText})]";
                tooltips.Add(new TooltipLine(Mod, "SequenceReq", text));
            }
        }

        // ==========================================
        // 4. 核心功能：智能配方生成器
        // ==========================================

        /// <summary>
        /// 自动注册"消耗石板"和"持有卡牌(免石板)"两种配方
        /// </summary>
        /// <param name="cardItemID">对应途径亵渎之牌的ID (无则填0)</param>
        /// <param name="ingredients">基础材料列表 (ID, 数量)</param>
        protected void CreateDualRecipe(int cardItemID, params (int itemID, int count)[] ingredients)
        {
            // --- 配方 A：普通版 (消耗亵渎石板) ---
            Recipe r1 = CreateRecipe();

            // 添加基础材料
            foreach (var (id, num) in ingredients)
            {
                r1.AddIngredient(id, num);
            }

            // 必加项：消耗 1 个亵渎石板
            // 确保你已经引用了 zhashi.Content.Items 命名空间
            r1.AddIngredient(ModContent.ItemType<BlasphemySlate>(), 1);
            r1.AddTile(TileID.Bottles);
            r1.Register();

            // --- 配方 B：权柄版 (持有亵渎之牌，不消耗石板) ---
            if (cardItemID > 0)
            {
                Recipe r2 = CreateRecipe();

                foreach (var (id, num) in ingredients)
                {
                    r2.AddIngredient(id, num);
                }

                // 添加条件：背包中有对应的牌
                r2.AddCondition(new Condition(
                    Terraria.Localization.Language.GetText($"Mods.zhashi.Items.{ModContent.GetModItem(cardItemID).Name}.DisplayName"),
                    () => Main.LocalPlayer.HasItem(cardItemID)
                ));

                r2.AddTile(TileID.Bottles);
                r2.Register();
            }
        }

        // ==========================================
        // 5. 辅助方法：序列检查逻辑
        // ==========================================
        private bool CheckSequenceRequirement(LotMPlayer p)
        {
            // 根据途径名检查对应的变量
            if (Pathway == "Hunter") return p.currentHunterSequence <= RequiredSequence;
            if (Pathway == "Fool") return p.currentFoolSequence <= RequiredSequence;
            if (Pathway == "Giant") return p.currentSequence <= RequiredSequence; // 巨人途径变量名特殊
            if (Pathway == "Moon") return p.currentMoonSequence <= RequiredSequence;
            if (Pathway == "Marauder") return p.currentMarauderSequence <= RequiredSequence;
            if (Pathway == "Sun") return p.currentSunSequence <= RequiredSequence;

            return false;
        }
    }
}