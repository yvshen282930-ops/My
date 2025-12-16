using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Fool
{
    public class FacelessPotion : LotMItem
    {
        public override string Pathway => "Fool";
        public override int RequiredSequence => 7; // 必须是魔术师

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
            Item.rare = ItemRarityID.LightRed; // 序列6级别
            Item.value = Item.sellPrice(gold: 10);
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();
                modPlayer.currentFoolSequence = 6; // 晋升序列6

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, player.position);
                Main.NewText("你忘记了自己的模样，又似乎记起了所有人的模样...", 200, 200, 255);
                Main.NewText("晋升成功！序列6：无面人！", 50, 255, 50);
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.RottenChunk, 5) // 异变脑垂体 (腐肉)
                .AddIngredient(ItemID.Bone, 10)       // 人皮幽影特性 (骨头，方便肉山前/后获取)
                .AddIngredient(ItemID.Deathweed, 1)   // 黑色曼陀罗
                .AddIngredient(ItemID.JungleSpores, 3)// 龙牙草
                .AddIngredient(ItemID.SharkFin, 1)    // 深海娜迦头发
                .AddTile(TileID.Bottles)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();

            // 猩红版本配方
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.Vertebrae, 5)   // 异变脑垂体 (脊椎)
                .AddIngredient(ItemID.Bone, 10)
                .AddIngredient(ItemID.Deathweed, 1)
                .AddIngredient(ItemID.JungleSpores, 3)
                .AddIngredient(ItemID.SharkFin, 1)
                .AddTile(TileID.Bottles)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}