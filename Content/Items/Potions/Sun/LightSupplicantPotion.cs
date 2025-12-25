using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace zhashi.Content.Items.Potions.Sun
{
    public class LightSupplicantPotion : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 26;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.useTurn = true;
            Item.maxStack = 99;
            Item.consumable = true;
            Item.rare = ItemRarityID.Green; // 序列8
            Item.value = Item.buyPrice(gold: 1);
            Item.buffType = BuffID.WellFed;
            Item.buffTime = 3600;
        }

        public override bool CanUseItem(Player player)
        {
            // 必须是序列9 歌颂者 才能服用
            return player.GetModPlayer<LotMPlayer>().currentSunSequence == 9;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                var p = player.GetModPlayer<LotMPlayer>();
                p.currentSunSequence = 8; // 晋升为祈光人
                Main.NewText("你感觉到炽热的光辉在体内流淌，你成为了祈光人！", 255, 215, 0);
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.FallenStar, 3)
                .AddIngredient(ItemID.Fireblossom, 1)

                // 主材料：岩浆石 (熔浆巨怪之心) - 保持不变
                .AddIngredient(ItemID.MagmaStone, 1)
                // 辅材料：红葡萄酒 -> 麦芽酒 - 保持不变
                .AddIngredient(ItemID.Ale, 1)
                // 辅材料：金边太阳花 -> 向日葵 - 保持不变
                .AddIngredient(ItemID.Sunflower, 1)
                // 辅材料：附子汁液 -> 水叶草 - 保持不变
                .AddIngredient(ItemID.Waterleaf, 1)
                // 辅材料：太阳信仰物品 -> 金锭 - 保持不变
                .AddIngredient(ItemID.GoldBar, 1)

                .AddTile(TileID.Bottles)
                .Register();
        }
    }
}