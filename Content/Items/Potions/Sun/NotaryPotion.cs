using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

// 【修改点1】命名空间加了 .Sun，因为文件在这个文件夹里
namespace zhashi.Content.Items.Potions.Sun
{
    public class NotaryPotion : ModItem
    {
        // 【修改点2】把之前那行 public override string Texture => ... 删掉！
        // 只要你的图片名叫 NotaryPotion.png 且放在旁边，它就会自动读取。

        public override void SetStaticDefaults()
        {
            
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
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 5);
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

                if (modPlayer.currentSunSequence == 7)
                {
                    modPlayer.currentSunSequence = 6;
                    Main.NewText("你服用了魔药，晋升为序列6：公证人。", 255, 215, 0);
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item29, player.position);
                    return true;
                }
                else
                {
                    Main.NewText("你的灵性不足以容纳这份特性，或者你无需再次服用。", 150, 150, 150);
                    return false;
                }
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.HallowedBar, 5)
                .AddIngredient(ItemID.SoulofFlight, 3)
                .AddIngredient(ItemID.PixieDust, 10)
                .AddIngredient(ItemID.Sunflower, 2)
                .AddIngredient(ItemID.BottledWater, 1)
                .AddTile(TileID.Bottles)
                .Register();
        }
    }
}