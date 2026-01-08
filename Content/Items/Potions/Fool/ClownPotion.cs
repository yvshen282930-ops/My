using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Fool
{
    public class ClownPotion : LotMItem
    {
        public override string Pathway => "Fool";
        public override int RequiredSequence => 9; // 必须是序列9才能喝

        public override void SetStaticDefaults()
        {
            // 名字和描述在HJSON中设置
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
            Item.rare = ItemRarityID.Green; // 序列8稍微稀有一点
            Item.value = Item.sellPrice(gold: 1);
            Item.buffType = BuffID.Tipsy; // 喝完有点晕（小丑的癫狂感）
            Item.buffTime = 600;
        }

        public override bool CanUseItem(Player player)
        {
            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();
            // 只有当前是序列9（占卜家）才能晋升
            return modPlayer.baseFoolSequence == 9;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();
                modPlayer.baseFoolSequence = 8; // 晋升为序列8

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, player.position);
                Main.NewText("你的嘴角不自觉地上扬，世界在你眼中变得滑稽而清晰...", 255, 165, 0);
                Main.NewText("晋升成功！序列8：小丑！", 50, 255, 50);
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)   // 纯水
                .AddIngredient(ItemID.Stinger, 1)        // 灰山羊独角结晶 (替代：毒刺)
                .AddIngredient(ItemID.JungleRose, 1)     // 人脸玫瑰 (替代：丛林玫瑰)
                .AddIngredient(ItemID.Blinkroot, 1)      // 曼陀罗 (替代：闪耀根)
                .AddIngredient(ItemID.Sunflower, 1)      // 太阳花粉 (替代：向日葵)
                .AddIngredient(ItemID.Moonglow, 1)       // 金斗篷草 (替代：月光草)
                .AddIngredient(ItemID.Deathweed, 1)      // 毒堇汁 (替代：死亡草)
                .AddTile(TileID.Bottles)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}