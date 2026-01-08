using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content; // 确保引用了 LotMPlayer 所在的命名空间

namespace zhashi.Content.Items.Potions.Hunter
{
    public class IronBloodedKnightPotion : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 30;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.UseSound = SoundID.Item3;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.maxStack = 99;
            Item.consumable = true; // 只有在 UseItem 返回 true 时才会消耗
            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.buyPrice(gold: 50);
        }

        // === 1. 显示仪式进度 ===
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<LotMPlayer>();

            // 只有当玩家是序列5（收割者）时才显示进度
            if (modPlayer.baseHunterSequence == 5)
            {
                // 根据进度变色 (红色未完成，绿色已完成)
                string statusColor = (modPlayer.ironBloodRitualProgress >= LotMPlayer.IRON_BLOOD_RITUAL_TARGET) ? "00FF00" : "FF0000";
                string progressText = $"[c/{statusColor}:征服进度: {modPlayer.ironBloodRitualProgress} / {LotMPlayer.IRON_BLOOD_RITUAL_TARGET}]";

                tooltips.Add(new TooltipLine(Mod, "RitualDesc", "晋升仪式: 在至少有5名随从(召唤物)的情况下，无死亡累计击杀100个敌人。"));
                tooltips.Add(new TooltipLine(Mod, "RitualProgress", progressText));
            }
            else if (modPlayer.baseHunterSequence > 5)
            {
                tooltips.Add(new TooltipLine(Mod, "RitualHint", "[c/808080:需要序列5才能查看仪式进度]"));
            }
        }

        // === 2. 基础使用检查 ===
        public override bool CanUseItem(Player player)
        {
            // 这里只做最基本的检查，详细检查放在 UseItem 里以便发送提示文本
            return true;
        }

        // === 3. 核心晋升逻辑 ===
        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer) return false;

            var modPlayer = player.GetModPlayer<LotMPlayer>();

            // --- 情况 A: 已经是高序列 (序列4及以上) ---
            if (modPlayer.baseHunterSequence <= 4)
            {
                Main.NewText("你已是铁血骑士或更高序列，无需再次服用。", 200, 200, 200);
                return false; // 【不消耗】
            }

            // --- 情况 B: 序列过低 (序列6及以下) ---
            if (modPlayer.baseHunterSequence > 5)
            {
                Main.NewText("你的灵性还不足以容纳这份魔药，请先成为【收割者】。", 200, 50, 50);
                return false; // 【不消耗】
            }

            // --- 情况 C: 正确序列 (序列5)，检查仪式 ---
            if (modPlayer.baseHunterSequence == 5)
            {
                // 检查进度是否达标
                // 注意：这里只检查了进度变量。
                // 确保你的 LotMPlayer 中写了逻辑：如果玩家死亡，ironBloodRitualProgress 归零！
                if (modPlayer.ironBloodRitualProgress < LotMPlayer.IRON_BLOOD_RITUAL_TARGET)
                {
                    Main.NewText("你的军队还未经历足够的铁与血...", 255, 50, 50);
                    Main.NewText("提示：需要保持5名随从并不死击杀100个敌人。", 200, 200, 200);
                    return false; // 【不消耗】仪式未完成，保留魔药
                }

                // --- 晋升成功 ---
                modPlayer.baseHunterSequence = 4;

                // 播放音效和特效
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, player.position);

                // 成功提示
                Main.NewText("火焰与钢铁在你体内重铸！", 255, 69, 0);
                Main.NewText("晋升成功：序列4 铁血骑士！", 255, 215, 0); // 金色
                Main.NewText("能力：【钢铁之躯】 | 【火焰化】(K) | 【集众】(L)", 255, 255, 255);

                return true; // 【消耗】返回 true 代表物品使用成功并减少堆叠
            }

            return false;
        }

        // === 4. 配方 ===
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.BeetleHusk, 10)   // 甲虫外壳
                .AddIngredient(ItemID.HellstoneBar, 10) // 狱岩锭
                .AddIngredient(ItemID.SoulofMight, 5)   // 力量之魂
                .AddIngredient(ItemID.SoulofSight, 5)   // 视域之魂
                .AddTile(TileID.Bottles)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1) // 亵渎石板
                .Register();
        }
    }
}