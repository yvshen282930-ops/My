using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Sun
{
    public class PriestPotion : ModItem
    {
        public override void SetStaticDefaults()
        {
            // 名字去 .hjson 里设置
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
            Item.rare = ItemRarityID.LightRed; // 序列5 浅红
            Item.value = Item.sellPrice(gold: 10);
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

                // 只有序列6才能喝
                if (modPlayer.baseSunSequence == 6)
                {
                    // === 【修改】仪式判定：净化 100 个不死生物 ===
                    // 读取你在 LotMPlayer 里写的变量
                    if (modPlayer.purificationProgress >= LotMPlayer.PURIFICATION_RITUAL_TARGET)
                    {
                        modPlayer.baseSunSequence = 5;
                        Main.NewText("你在无尽的战斗中领悟了光的真谛...", 255, 215, 0);
                        Main.NewText("晋升成功！序列5：光之祭司。", 255, 215, 0);
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item29, player.position);

                        // 播放一圈金色特效
                        for (int i = 0; i < 100; i++)
                        {
                            Dust d = Dust.NewDustPerfect(player.Center, DustID.GoldFlame, Main.rand.NextVector2Circular(10, 10), 0, default, 3f);
                            d.noGravity = true;
                        }
                        return true;
                    }
                    else
                    {
                        // 提示进度
                        Main.NewText($"仪式未完成！你需要净化更多的不死生物 ({modPlayer.purificationProgress}/{LotMPlayer.PURIFICATION_RITUAL_TARGET})。", 255, 50, 50);
                        Main.NewText("提示：去击杀僵尸、骷髅或亡灵类怪物。", 200, 200, 200);
                        return false;
                    }
                }
                else
                {
                    Main.NewText("您的序列不符合要求。", 150, 150, 150);
                    return false;
                }
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SunStone, 1)
                .AddIngredient(ItemID.Diamond, 5)
                .AddIngredient(ItemID.HallowedBar, 10)
                .AddIngredient(ItemID.SoulofLight, 10)
                .AddIngredient(ItemID.PixieDust, 20) // 修改：加点精灵尘更像光之祭司
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .AddIngredient(ItemID.BottledWater, 1)
                .AddTile(TileID.Bottles)
                .Register();
        }
    }
}