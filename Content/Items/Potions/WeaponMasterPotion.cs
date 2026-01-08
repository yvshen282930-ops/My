using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions
{
    public class WeaponMasterPotion : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 26;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.UseSound = SoundID.Item3;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.maxStack = 99;
            Item.consumable = true;
            Item.rare = ItemRarityID.Orange; // 橙色稀有度 (困难模式前的高级物品)
            Item.value = Item.buyPrice(gold: 3);
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();

            // 必须是序列8才能晋升序列7
            if (modPlayer.baseSequence == 8)
            {
                modPlayer.baseSequence = 7;
                // 播放更强的吼声
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, player.position);
                Main.NewText("你对武器的理解达到了新的高度... 晋升成功：序列7 武器大师！", 255, 165, 0); // 橙色字
                return true;
            }
            else if (modPlayer.baseSequence > 8)
            {
                Main.NewText("你还未掌握格斗家的技巧，无法晋升。", 200, 50, 50);
                return true;
            }
            else
            {
                Main.NewText("你已经是更高序列的存在，无需重复服用。", 200, 200, 200);
                return true;
            }
        }

        public override void AddRecipes()
        {
            // 配方1：腐化世界 (暗影鳞片)
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.ShadowScale, 10) // 击败世界吞噬者获得
                .AddIngredient(ItemID.Obsidian, 5)     // 黑曜石 (代表坚固)
                .AddIngredient(ItemID.Stinger, 3)      // 毒刺 (丛林)
                .AddTile(TileID.Bottles)
                .Register();

            // 配方2：猩红世界 (组织样本)
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.TissueSample, 10)// 击败克苏鲁之脑获得
                .AddIngredient(ItemID.Obsidian, 5)
                .AddIngredient(ItemID.Stinger, 3)
                .AddTile(TileID.Bottles)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}