using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content.Buffs;

namespace zhashi.Content.Items.Potions.Marauder
{
    public class WormOfTimePotion : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 32;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.maxStack = 99;
            Item.consumable = true;
            Item.rare = ItemRarityID.Red; // 序列1 天使之王/旧日
            Item.value = Item.sellPrice(platinum: 1);
        }

        public override bool CanUseItem(Player player)
        {
            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

            if (modPlayer.currentMarauderSequence != 2) return false;

            // 检查仪式：在城镇中维持混乱7分钟 (25200帧)
            if (modPlayer.wormRitualTimer < LotMPlayer.WORM_RITUAL_TARGET)
            {
                int minLeft = (LotMPlayer.WORM_RITUAL_TARGET - modPlayer.wormRitualTimer) / 3600;
                Main.NewText($"仪式未完成：这座城市还未完全陷入时光的混乱... (还需约 {minLeft + 1} 分钟)", 200, 50, 50);
                return false;
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();
            if (player.itemAnimation > 0 && player.itemTime == 0)
            {
                modPlayer.currentMarauderSequence = 1;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, player.position);

                Main.NewText("你看见了斑驳的石壁，听见了古老的钟声，你成为了时间的寄生者...", 0, 255, 255);
                Main.NewText("晋升成功：序列1 时之虫！", 255, 215, 0);

                // 获得神性提升
                modPlayer.spiritualityMax += 5000;
                player.statLifeMax2 += 1000;

                return true;
            }
            return null;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.LunarBar, 5)          // 【修复】这里改成了 LunarBar (夜明锭)
                .AddIngredient(ItemID.FragmentStardust, 10) // 星尘碎片
                .AddIngredient(ItemID.Timer1Second, 1)      // 1秒计时器
                .AddIngredient(ItemID.Book, 1)              // 书
                .AddIngredient(ItemID.Ectoplasm, 5)         // 灵气
                .AddTile(TileID.LunarCraftingStation)       // 远古操纵机
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}