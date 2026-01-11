using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;
using zhashi.Content.Items;

namespace zhashi.Content.Items.Potions.Sun
{
    public class UnshadowedPotion : ModItem
    {
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
            Item.rare = ItemRarityID.Yellow; // 序列4 黄色稀有度
            Item.value = Item.sellPrice(gold: 20);
        }

        public override bool CanUseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

                // 1. 检查序列：必须正好是序列 5
                if (modPlayer.baseSunSequence != 5)
                {
                    Main.NewText($"[晋升失败] 序列不匹配！你当前是序列 {modPlayer.baseSunSequence}，必须是序列 5 才能晋升。", 255, 50, 50);
                    return false;
                }

                // 2. 检查圣物：背包里是否有【无暗十字】
                if (!player.HasItem(ModContent.ItemType<UnshadowedCross>()))
                {
                    Main.NewText("[晋升失败] 缺少关键圣物：你的背包里没有【无暗十字】。", 255, 50, 50);
                    return false;
                }

                // 3. 检查仪式：审判进度
                // 确保 LotMPlayer 里定义了 JUDGMENT_RITUAL_TARGET (通常是20)
                if (modPlayer.judgmentProgress < LotMPlayer.JUDGMENT_RITUAL_TARGET)
                {
                    Main.NewText($"[晋升失败] 仪式未完成！你需要审判更多强敌 ({modPlayer.judgmentProgress}/{LotMPlayer.JUDGMENT_RITUAL_TARGET})。", 255, 50, 50);
                    Main.NewText("提示：去攻击 Boss 或 血量>2000 的怪物。", 200, 200, 200);
                    return false;
                }
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

                // 执行晋升
                modPlayer.baseSunSequence = 4;

                // 播放成功特效和音效
                Main.NewText("情感被剥离又回归，你的身体化作了纯粹的光...", 255, 215, 0);
                Main.NewText("晋升半神！序列4：无暗者。", 255, 69, 0);
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item29, player.position);

                for (int i = 0; i < 200; i++)
                {
                    Vector2 speed = Main.rand.NextVector2Circular(15, 15);
                    Dust.NewDustPerfect(player.Center, DustID.SolarFlare, speed, 0, default, 3f).noGravity = true;
                }
                return true;
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SolarTablet, 5)
                .AddIngredient(ItemID.Ruby, 5)
                .AddIngredient(ItemID.Topaz, 5)
                .AddIngredient(ItemID.LavaBucket, 1)
                .AddIngredient(ItemID.SoulofSight, 5) // 视域之魂
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .AddIngredient(ItemID.BottledWater, 1)
                .AddTile(TileID.Bottles)
                .Register();
        }
    }
}