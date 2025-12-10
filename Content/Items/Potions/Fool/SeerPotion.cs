using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using zhashi.Content; // 引用核心 Player 类

namespace zhashi.Content.Items.Potions.Fool
{
    public class SeerPotion : LotMItem
    {
        // 设定途径为 "Fool" (愚者)
        public override string Pathway => "Fool";
        public override int RequiredSequence => 10; // 序列9通常没有前置要求，或者要求是普通人(10)

        public override void SetStaticDefaults()
        {
            // 建议在 hjson 中设置 DisplayName 和 Tooltip
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
            Item.buffType = BuffID.WellFed;
            Item.buffTime = 300;
        }

        public override bool CanUseItem(Player player)
        {
            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();
            // 只有普通人(不是非凡者)才能喝，或者您可以根据需求修改限制
            return !modPlayer.IsBeyonder;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();
                modPlayer.currentFoolSequence = 9; // 晋升为序列9

                // 播放音效和文字提示
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, player.position);
                Main.NewText("你感觉到灵感在脑海中迸发，眼前的世界多了许多虚幻的丝线...", 100, 100, 255);
                Main.NewText("晋升成功！序列9：占卜家！", 50, 255, 50);
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1) // 纯水
                .AddIngredient(ItemID.BlackInk, 1)     // 拉瓦章鱼血液 (替代品：黑墨水)
                .AddIngredient(ItemID.FallenStar, 1)   // 星水晶 (替代品：坠落之星)
                .AddIngredient(ItemID.Daybloom, 1)     // 夜香草 (替代品：太阳花)
                .AddIngredient(ItemID.Blinkroot, 1)    // 金薄荷 (替代品：闪耀根)
                .AddIngredient(ItemID.Deathweed, 1)    // 龙血草 (替代品：死亡草)
                .AddTile(TileID.Bottles)
                .Register();
        }
    }
}