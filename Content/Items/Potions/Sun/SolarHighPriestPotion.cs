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
            return player.GetModPlayer<LotMPlayer>().baseSunSequence == 8;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                var p = player.GetModPlayer<LotMPlayer>();
                p.baseSunSequence = 7;
                Main.NewText("你感受到了太阳的威严，晋升为太阳神官！", 255, 215, 0);
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Fireblossom, 3)  
                .AddIngredient(ItemID.HellstoneBar, 5) 
                .AddIngredient(ItemID.HoneyBlock, 5)   
                .AddIngredient(ItemID.LavaBucket, 1)   
                .AddIngredient(ItemID.GoldBar, 2)     
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .AddTile(TileID.Bottles)
                .Register();
        }
    }
}