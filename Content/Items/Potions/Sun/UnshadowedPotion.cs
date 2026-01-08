using Microsoft.Xna.Framework;
using System.Collections.Generic;
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
            Item.consumable = true; // 确保开启消耗
            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(gold: 20);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

            bool seqReady = modPlayer.baseSunSequence == 5;
            string seqColor = seqReady ? "00FF00" : "FF0000";
            tooltips.Add(new TooltipLine(Mod, "SeqReq", $"[c/{seqColor}:条件1: 需序列5 光之祭司]"));

            bool ritualReady = modPlayer.judgmentProgress >= LotMPlayer.JUDGMENT_RITUAL_TARGET;
            string ritualColor = ritualReady ? "00FF00" : "FF0000";
            tooltips.Add(new TooltipLine(Mod, "RitualReq", $"[c/{ritualColor}:条件2: 审判强敌 ({modPlayer.judgmentProgress}/{LotMPlayer.JUDGMENT_RITUAL_TARGET})]"));

            bool hasCross = player.HasItem(ModContent.ItemType<Items.UnshadowedCross>());
            string itemColor = hasCross ? "00FF00" : "FF0000";
            string itemStatus = hasCross ? "已持有" : "缺失";
            tooltips.Add(new TooltipLine(Mod, "ItemReq", $"[c/{itemColor}:条件3: 需持有 无暗十字 ({itemStatus})]"));

            if (!ritualReady)
            {
                tooltips.Add(new TooltipLine(Mod, "Hint", "提示: 使用公证/审判技能击败Boss或高血量精英怪"));
            }
        }

        // 1. 【拦截逻辑】在这里检查所有条件，不满足连喝动作都做不出来
        public override bool CanUseItem(Player player)
        {
            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

            // 检查序列
            if (modPlayer.baseSunSequence != 5) return false;

            // 检查圣物
            if (!player.HasItem(ModContent.ItemType<Items.UnshadowedCross>())) return false;

            // 检查仪式
            if (modPlayer.judgmentProgress < LotMPlayer.JUDGMENT_RITUAL_TARGET) return false;

            return true;
        }

        public override void OnConsumeItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

                // 执行晋升
                modPlayer.baseSunSequence = 4;
                modPlayer.currentSunSequence = 4;

                Main.NewText("情感被剥离又回归，你的身体化作了纯粹的光...", 255, 215, 0);
                Main.NewText("晋升半神！序列4：无暗者。", 255, 69, 0);
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item29, player.position);

                // 半神特效
                for (int i = 0; i < 200; i++)
                {
                    Vector2 speed = Main.rand.NextVector2Circular(15, 15);
                    Dust.NewDustPerfect(player.Center, DustID.SolarFlare, speed, 0, default, 3f).noGravity = true;
                }
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SolarTablet, 5)
                .AddIngredient(ItemID.Ruby, 5)
                .AddIngredient(ItemID.Topaz, 5)
                .AddIngredient(ItemID.LavaBucket, 1)
                .AddIngredient(ItemID.SoulofSight, 5)
                .AddIngredient(ItemID.LifeFruit, 3)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .AddIngredient(ItemID.BottledWater, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}