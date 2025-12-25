using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Sun
{
    public class LightSeekerPotion : ModItem
    {
        public override void SetStaticDefaults()
        {
            // 名字在 .hjson 中设置
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 40;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.maxStack = 30;
            Item.consumable = true;
            Item.rare = ItemRarityID.Red; // 红色 (序列2 天使)
            Item.value = Item.sellPrice(platinum: 5);
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

                if (modPlayer.currentSunSequence == 3)
                {
                    // === 仪式：锚定光明 ===

                    // 1. 必须是白天
                    bool isDay = Main.dayTime;

                    // 2. 【核心修复】检查背包里是否有圣物，而不是检查手持
                    // 只要你背包里带着 破晓之光(DayBreak) 或 日耀喷发剑(SolarEruption) 就算通过
                    bool hasArtifact = player.HasItem(ItemID.DayBreak) || player.HasItem(ItemID.SolarEruption);

                    // 3. 状态完好 (满血)
                    bool isHealthy = player.statLife >= player.statLifeMax2;

                    if (isDay && hasArtifact && isHealthy)
                    {
                        modPlayer.currentSunSequence = 2;
                        Main.NewText("你将核心的正义与太阳的权柄捆绑，化作了一道纯净的光...", 255, 255, 0);
                        Main.NewText("晋升天使！序列2：逐光者。", 255, 69, 0);
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item29, player.position);

                        // 天使晋升特效
                        for (int i = 0; i < 200; i++)
                        {
                            Vector2 speed = Main.rand.NextVector2Circular(20, 20);
                            Dust d = Dust.NewDustPerfect(player.Center, DustID.SolarFlare, speed, 0, default, 4f);
                            d.noGravity = true;
                        }
                        return true;
                    }
                    else
                    {
                        if (!isDay) Main.NewText("仪式失败：必须在烈日下进行。", 150, 150, 150);
                        else if (!hasArtifact) Main.NewText("仪式失败：背包中需携带天使级太阳圣物(破晓/日耀喷发)作为道标。", 150, 150, 150);
                        else Main.NewText("仪式失败：你需要保持最完美的状态(满血)以维持意识。", 150, 150, 150);
                        return false;
                    }
                }
                else
                {
                    Main.NewText("你的灵性不足以容纳这份权柄。", 150, 150, 150);
                    return false;
                }
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.FragmentSolar, 20)
                .AddIngredient(ItemID.SoulofLight, 20)
                .AddIngredient(ItemID.LifeFruit, 5)
                .AddIngredient(ItemID.Ectoplasm, 10)
                .AddIngredient(ItemID.Book, 1)
                .AddIngredient(ItemID.BottledWater, 1)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}