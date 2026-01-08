using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Sun
{
    public class LightSupplicantPotion : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 26;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.useTurn = true;
            Item.maxStack = 99;
            Item.consumable = true;
            Item.rare = ItemRarityID.Green; // 序列8
            Item.value = Item.buyPrice(gold: 1);
            Item.buffType = BuffID.WellFed;
            Item.buffTime = 3600;
        }

        // 【新增】动态提示：直观显示条件满足情况
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

            // 检查是否满足前置序列 (序列9 歌颂者)
            bool seqReady = modPlayer.baseSunSequence == 9;
            string seqColor = seqReady ? "00FF00" : "FF0000"; // 绿/红
            string statusText = seqReady ? "已满足" : "未满足";

            tooltips.Add(new TooltipLine(Mod, "SeqReq", $"[c/{seqColor}:条件: 需序列9 歌颂者 ({statusText})]"));
        }

        // 【核心拦截】只有序列9才能用，防止误用
        public override bool CanUseItem(Player player)
        {
            return player.GetModPlayer<LotMPlayer>().baseSunSequence == 9;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                var p = player.GetModPlayer<LotMPlayer>();

                // 双重保险检查
                if (p.baseSunSequence == 9)
                {
                    // 1. 修改 Base (存档用)
                    p.baseSunSequence = 8;
                    // 2. 同步 Current (立刻生效用)
                    p.currentSunSequence = 8;

                    Main.NewText("你感觉到炽热的光辉在体内流淌，你成为了祈光人！", 255, 215, 0);

                    // 3. 播放音效
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item3, player.position);

                    // (可选) 播放一点简单的晋升粒子特效
                    for (int i = 0; i < 20; i++)
                    {
                        Dust.NewDust(player.position, player.width, player.height, DustID.GoldFlame, 0, 0, 100, default, 1.5f);
                    }
                }
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.FallenStar, 3)
                .AddIngredient(ItemID.Fireblossom, 1)
                .AddIngredient(ItemID.MagmaStone, 1)
                .AddIngredient(ItemID.Ale, 1)
                .AddIngredient(ItemID.Sunflower, 1)
                .AddIngredient(ItemID.Waterleaf, 1)
                .AddIngredient(ItemID.GoldBar, 1)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1) // 确保引用了亵渎石板
                .AddTile(TileID.Bottles)
                .Register();
        }
    }
}