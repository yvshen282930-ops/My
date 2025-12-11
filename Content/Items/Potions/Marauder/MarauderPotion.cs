using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Marauder
{
    public class MarauderPotion : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("魔药：偷盗者");
            // Tooltip.SetDefault("序列9 偷盗者\n" +
            //                  "赋予使用者敏捷的身手与卓越的观察力。\n" +
            //                  "开启错误途径的起始。\n" +
            //                  "“窃取他人的命运，或者被命运窃取。”");
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 26;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.maxStack = 30;
            Item.consumable = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 50);
            Item.buffType = BuffID.WellFed;
            Item.buffTime = 600;
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();

            // --- 核心修复逻辑 ---
            // 判定：如果玩家是凡人 (10)，或者是其他更弱的状态（大于9），才允许晋升
            if (modPlayer.currentMarauderSequence > 9)
            {
                // 1. 晋升为序列9
                modPlayer.currentMarauderSequence = 9;

                // 2. 视觉/听觉反馈
                Main.NewText("你的手指变得灵活，眼中闪烁着奇异的光芒... 你成为了[偷盗者]！", 50, 50, 200);
                return true;
            }
            // 如果已经是序列9或更强 (<= 9)
            else
            {
                Main.NewText("你体内的非凡特性排斥这份魔药，你已经不需要它了。", 150, 150, 150);
                return true; // 仍然消耗，或者改为 return false 不消耗
            }
        }

        public override void AddRecipes()
        {
            // 遵循石板上的配方：水瓶 + 毒刺 + 粉凝胶 + 蓝宝石
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.Stinger, 1)     // 毒刺
                .AddIngredient(ItemID.PinkGel, 1)     // 粉凝胶
                .AddIngredient(ItemID.Sapphire, 1)    // 蓝宝石
                .AddTile(TileID.Bottles)
                .Register();
        }
    }
}