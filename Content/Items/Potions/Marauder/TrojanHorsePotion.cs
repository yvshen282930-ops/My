using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content.Buffs;

namespace zhashi.Content.Items.Potions.Marauder
{
    public class TrojanHorsePotion : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 32;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.maxStack = 99;
            Item.consumable = true;
            Item.rare = ItemRarityID.Yellow; // 序列2 天使/命运木马
            Item.value = Item.sellPrice(gold: 50);
        }

        public override bool CanUseItem(Player player)
        {
            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

            if (modPlayer.baseMarauderSequence != 3) return false;

            // 检查仪式：累积寄生5分钟
            if (modPlayer.trojanRitualTimer < LotMPlayer.TROJAN_RITUAL_TARGET)
            {
                int secondsLeft = (LotMPlayer.TROJAN_RITUAL_TARGET - modPlayer.trojanRitualTimer) / 60;
                Main.NewText($"仪式未完成：你需要寄生并顶替他人身份更长时间... (剩余 {secondsLeft} 秒)", 200, 50, 50);
                return false;
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();
            if (player.itemAnimation > 0 && player.itemTime == 0)
            {
                modPlayer.baseMarauderSequence = 2;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, player.position);

                Main.NewText("你感觉自己沉入了命运的长河，无数的可能性在眼前展开...", 175, 238, 238);
                Main.NewText("晋升成功：序列2 命运木马！", 255, 215, 0);

                // 获得一些无敌时间适应
                player.immune = true;
                player.immuneTime = 300;

                return true;
            }
            return null;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.Ectoplasm, 10)       // 主材料
                .AddIngredient(ItemID.SoulofLight, 10)     // 光
                .AddIngredient(ItemID.SoulofNight, 10)     // 暗
                .AddIngredient(ItemID.TruffleWorm, 1)      // 灵之虫
                .AddIngredient(ItemID.EnchantedNightcrawler, 1) // 星之虫
                .AddIngredient(ItemID.HolyWater, 5)        // 古老河水
                .AddTile(TileID.Bottles)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}