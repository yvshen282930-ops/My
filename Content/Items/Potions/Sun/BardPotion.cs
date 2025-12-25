using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content.Items.Materials; // 假设你有自定义材料命名空间

namespace zhashi.Content.Items.Potions.Sun
{
    public class BardPotion : ModItem
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
            Item.rare = ItemRarityID.Blue; // 序列9通常是蓝色稀有度
            Item.value = Item.buyPrice(silver: 10);
            Item.buffType = BuffID.WellFed; // 喝下去有个临时饱食度
            Item.buffTime = 3600;
        }

        public override bool CanUseItem(Player player)
        {
            // 只有普通人 (序列10) 才能服用序列9
            return player.GetModPlayer<LotMPlayer>().currentSunSequence == 10;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                var p = player.GetModPlayer<LotMPlayer>();
                p.currentSunSequence = 9; // 晋升为歌颂者
                Main.NewText("你感觉喉咙变得灼热，心中充满了勇气！", 255, 215, 0);
            }
            return true;
        }

        public override void AddRecipes()
        {
            // 配方：尽量还原你的描述
            CreateRecipe()
                // 主材料：结晶太阳花 -> 向日葵 (Sunflower)
                .AddIngredient(ItemID.Sunflower, 1)
                // 主材料：火石鸟尾羽 -> 丛林孢子 + 羽毛 (模拟)
                .AddIngredient(ItemID.Feather, 1)
                // 辅材料：红葡萄酒 -> 麦芽酒 (Ale)
                .AddIngredient(ItemID.Ale, 1)
                // 辅材料：仲夏草 -> 太阳花 (Daybloom)
                .AddIngredient(ItemID.Daybloom, 1)
                // 辅材料：精灵暗叶 -> 月光草 (Moonglow)
                .AddIngredient(ItemID.Moonglow, 1)
                .AddTile(TileID.Bottles)
                .Register();
        }
    }
}