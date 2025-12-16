using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using zhashi.Content.Items.Materials;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Moon
{
    public class VampirePotion : LotMItem
    {
        public override void SetStaticDefaults() { }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 30;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.maxStack = 30;
            Item.consumable = true;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold: 3);
        }

        public override bool CanUseItem(Player player)
        {
            // 必须是 序列8：驯兽师
            return player.GetModPlayer<LotMPlayer>().currentMoonSequence == 8;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();
                modPlayer.currentMoonSequence = 7; // 晋升

                CombatText.NewText(player.getRect(), Color.DarkRed, "晋升：吸血鬼", true);
                Main.NewText("你感到心脏剧烈跳动，对鲜血产生了渴望...", 220, 20, 60);

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item103, player.position);
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                // 两种邪恶生物材料二选一 (暗影鳞片 或 组织样本)
                .AddRecipeGroup("IronBar", 1) // 暂时用铁锭代替，你可以换成 RecipeGroup.AnyEvilBar
                .AddIngredient(ItemID.Bone, 10)          // 骨头
                .AddIngredient(ItemID.WaterCandle, 1)    // 水蜡烛
                .AddIngredient(ItemID.Moonglow, 3)       // 月光草
                .AddIngredient(ItemID.Deathweed, 3)      // 死亡草
                .AddTile(TileID.Bottles)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}