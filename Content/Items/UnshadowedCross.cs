using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using zhashi.Content;

namespace zhashi.Content.Items
{
    public class UnshadowedCross : LotMItem
    {
        public override void SetStaticDefaults()
        {
            // 名称和描述由 Hjson 提供
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 45;
            Item.useTime = 45;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item29;
            Item.maxStack = 1;
            Item.consumable = false;   // 不消耗
            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(gold: 10);
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();
                bool loweredAny = false; // 标记是否有任何途径发生了降级
                string msg = "";

                // 定义一个局部函数来处理降级逻辑
                void DowngradePathway(ref int currentSeq, string pathwayName)
                {
                    // 如果不是凡人 (序列 < 10)
                    if (currentSeq < 10)
                    {
                        currentSeq++; // 数字+1 即序列降低 (如 1 -> 2, 9 -> 10)
                        loweredAny = true;

                        if (currentSeq >= 10)
                        {
                            currentSeq = 10;
                            msg += $"{pathwayName}途径特性已完全析出，回归平凡。\n";
                        }
                        else
                        {
                            msg += $"{pathwayName}途径序列降低为：序列{currentSeq}。\n";
                        }
                    }
                }

                // 检查并降低所有途径
                DowngradePathway(ref modPlayer.currentSequence, "巨人");
                DowngradePathway(ref modPlayer.currentHunterSequence, "猎人");
                DowngradePathway(ref modPlayer.currentMoonSequence, "月亮");
                DowngradePathway(ref modPlayer.currentFoolSequence, "愚者");
                DowngradePathway(ref modPlayer.currentMarauderSequence, "错误");

                // 如果有变化
                if (loweredAny)
                {
                    // 视觉与文字提示
                    CombatText.NewText(player.getRect(), Color.Gold, "特性析出", true);
                    if (!string.IsNullOrEmpty(msg)) Main.NewText(msg.TrimEnd(), 255, 215, 0);

                    // 播放神圣特效
                    for (int i = 0; i < 60; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2Circular(3f, 3f);
                        Dust d = Dust.NewDustPerfect(player.Center, DustID.GoldFlame, speed, 0, default, 2.5f);
                        d.noGravity = true;
                    }

                    // 如果彻底变成了凡人（所有途径都回到10），重置灵性等杂项
                    if (!modPlayer.IsBeyonder)
                    {
                        modPlayer.spiritualityCurrent = 100;
                        Main.NewText("你已彻底洗净了非凡特性，变回了普通人。", 200, 200, 200);
                    }
                }
                else
                {
                    Main.NewText("你已经是凡人了，没有任何非凡特性可以析出。", 150, 150, 150);
                }
            }
            return true;
        }
    }
}