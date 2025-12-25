using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace zhashi.Content.Items.Potions.Sun
{
    public class SolarHighPriestPotion : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 26;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 15; Item.useTime = 15;
            Item.useTurn = true; Item.maxStack = 99;
            Item.consumable = true;
            Item.rare = ItemRarityID.Orange; // 序列7
            Item.value = Item.buyPrice(gold: 5);
            Item.buffType = BuffID.WellFed; Item.buffTime = 3600;
        }

        public override bool CanUseItem(Player player)
        {
            return player.GetModPlayer<LotMPlayer>().currentSunSequence == 8;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                var p = player.GetModPlayer<LotMPlayer>();
                p.currentSunSequence = 7;
                Main.NewText("你感受到了太阳的威严，晋升为太阳神官！", 255, 215, 0);
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Fireblossom, 3)  // 黎明雄鸡红冠 (替代)
                .AddIngredient(ItemID.HellstoneBar, 5) // 光辉契灵树果实 (替代)
                .AddIngredient(ItemID.HoneyBlock, 5)   // 金手柑 (替代)
                .AddIngredient(ItemID.LavaBucket, 1)   // 凝固熔浆
                .AddIngredient(ItemID.GoldBar, 2)      // 太阳精油
                .AddTile(TileID.Bottles)
                .Register();
        }
    }
}