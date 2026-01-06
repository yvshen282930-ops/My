using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework; // 【核心修复】必须引用这个才能使用 Color
using zhashi.Content.Items.Materials;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Moon
{
    public class ApothecaryPotion : LotMItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("药师魔药");
            // Tooltip.SetDefault("服用后晋升为 序列9：药师\n获得灵视、抗毒体质与治疗能力");
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
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

                if (modPlayer.IsBeyonder)
                {
                    Main.NewText("你的灵性已定型...", 255, 50, 50);
                    return true;
                }

                modPlayer.currentMoonSequence = 9; // 现在这行肯定能跑到了

                CombatText.NewText(player.getRect(), Color.LightGreen, "晋升：药师", true);
                Main.NewText("你感觉自己对草药与生命有了更深的理解...", 100, 255, 100);
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, player.position);
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.Stinger, 3)
                .AddIngredient(ItemID.Glowstick, 7)
                .AddIngredient(ItemID.Deathweed, 1)
                .AddTile(TileID.Bottles)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}