using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Hunter
{
    public class PyromaniacPotion : ModItem
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
            Item.consumable = true;
            Item.rare = ItemRarityID.Orange; // 橙色
            Item.value = Item.buyPrice(gold: 3);
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();

            if (modPlayer.currentHunterSequence == 8)
            {
                modPlayer.currentHunterSequence = 7;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item74, player.position); // 火焰音效

                Main.NewText("你的血液仿佛沸腾了...", 255, 100, 0);
                Main.NewText("晋升成功：序列7 纵火家！", 255, 100, 0);
                Main.NewText("能力：【火焰亲和】(免火) | 【火球术】(按F键)", 255, 255, 255);
                return true;
            }
            else if (modPlayer.currentHunterSequence > 8)
            {
                Main.NewText("你还未成为挑衅者。", 200, 50, 50);
                return true;
            }
            else
            {
                Main.NewText("你已是纵火家。", 200, 200, 200);
                return true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.HellstoneBar, 5) // 狱石锭
                .AddIngredient(ItemID.Fireblossom, 3)  // 火焰花
                .AddIngredient(ItemID.ExplosivePowder, 10) // 爆炸粉
                .AddTile(TileID.Bottles)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}