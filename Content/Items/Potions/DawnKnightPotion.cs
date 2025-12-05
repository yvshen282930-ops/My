using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions
{
    public class DawnKnightPotion : ModItem
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
            Item.rare = ItemRarityID.Pink; // 粉色稀有度
            Item.value = Item.buyPrice(gold: 10);
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();

            if (modPlayer.currentSequence == 7)
            {
                modPlayer.currentSequence = 6;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, player.position);

                Main.NewText("晨曦的光芒照亮了你的灵魂... 晋升成功：序列6 黎明骑士！", 255, 215, 0);
                Main.NewText("获得技能：【黎明铠甲】 (按 Z 键开启/关闭)", 255, 255, 255);
                return true;
            }
            else if (modPlayer.currentSequence > 7)
            {
                Main.NewText("你还未成为武器大师，无法承受晨曦之力。", 200, 50, 50);
                return true;
            }
            else
            {
                Main.NewText("你的光芒已足够耀眼。", 200, 200, 200);
                return true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.HallowedBar, 5)   // 神圣锭
                .AddIngredient(ItemID.SoulofLight, 10) // 【修复】这里改成了单数的 SoulofLight
                .AddIngredient(ItemID.Daybloom, 5)      // 太阳花
                .AddTile(TileID.Bottles)
                .Register();
        }
    }
}