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

            // 【核心修复】
            // 使用 IsBeyonder 检查玩家是否已经是任何途径的非凡者
            // 如果 IsBeyonder 为 true，说明已经是某条途径的序列9或更高了，必须禁止服用
            if (modPlayer.IsBeyonder)
            {
                Main.NewText("你的灵性已定型，无法开启第二条途径，强行服用只会导致失控！", 255, 50, 50);
                return true; // 消耗物品但无效
            }

            // 只有当玩家完全是凡人时，才允许晋升
            modPlayer.currentMarauderSequence = 9;
            Main.NewText("你的手指变得灵活，眼中闪烁着奇异的光芒... 你成为了[偷盗者]！", 50, 50, 200);
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.Stinger, 1)      // 毒刺
                .AddIngredient(ItemID.PinkGel, 1)      // 粉凝胶
                .AddIngredient(ItemID.Sapphire, 1)     // 蓝宝石
                .AddTile(TileID.Bottles)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}