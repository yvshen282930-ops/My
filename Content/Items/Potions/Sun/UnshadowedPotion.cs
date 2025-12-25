using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Sun
{
    public class UnshadowedPotion : ModItem
    {
        public override void SetStaticDefaults()
        {
            // 名字去 .hjson 里写
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
            Item.rare = ItemRarityID.Yellow; // 序列4 黄色/金色品质
            Item.value = Item.sellPrice(gold: 20);
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

                if (modPlayer.currentSunSequence == 5)
                {
                    // === 【修改】仪式判定：审判 20 个强敌 ===
                    if (modPlayer.judgmentProgress >= LotMPlayer.JUDGMENT_RITUAL_TARGET)
                    {
                        modPlayer.currentSunSequence = 4;
                        Main.NewText("情感被剥离又回归，你的身体化作了纯粹的光...", 255, 215, 0);
                        Main.NewText("晋升半神！序列4：无暗者。", 255, 69, 0);
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item29, player.position);

                        // 半神特效：巨大的太阳爆炸视觉
                        for (int i = 0; i < 200; i++)
                        {
                            Vector2 speed = Main.rand.NextVector2Circular(15, 15);
                            Dust.NewDustPerfect(player.Center, DustID.SolarFlare, speed, 0, default, 3f).noGravity = true;
                        }
                        return true;
                    }
                    else
                    {
                        // 提示进度
                        Main.NewText($"仪式未完成！你需要审判更多的强敌 ({modPlayer.judgmentProgress}/{LotMPlayer.JUDGMENT_RITUAL_TARGET})。", 255, 50, 50);
                        Main.NewText("提示：使用公证/审判技能击败Boss或高血量精英怪。", 200, 200, 200);
                        return false;
                    }
                }
                else
                {
                    Main.NewText("你的灵性不足以容纳神性 (需序列5)。", 150, 150, 150);
                    return false;
                }
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SolarTablet, 5)   // 太阳石板碎片
                .AddIngredient(ItemID.Ruby, 5)
                .AddIngredient(ItemID.Topaz, 5)
                .AddIngredient(ItemID.LavaBucket, 1)
                .AddIngredient(ItemID.SoulofSight, 5)   // 新增：视域之魂(洞察)
                .AddIngredient(ItemID.LifeFruit, 3)     // 新增：生命果
                .AddIngredient(ItemID.BottledWater, 1)
                .AddTile(TileID.MythrilAnvil)           // 修改为秘银砧
                .Register();
        }
    }
}