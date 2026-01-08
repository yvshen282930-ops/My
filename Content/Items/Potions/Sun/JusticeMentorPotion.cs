using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Sun
{
    public class JusticeMentorPotion : ModItem
    {
        public override void SetStaticDefaults()
        {
            // 名字在 .hjson 中设置
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 36;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.maxStack = 30;
            Item.consumable = true;
            Item.rare = ItemRarityID.Cyan; // 序列3 圣者
            Item.value = Item.sellPrice(platinum: 1);
        }

        // 【新增】显示仪式条件和进度
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

            // 1. 序列检测
            bool seqReady = modPlayer.baseSunSequence == 4;
            string seqColor = seqReady ? "00FF00" : "FF0000";
            tooltips.Add(new TooltipLine(Mod, "SeqReq", $"[c/{seqColor}:条件1: 需序列4 无暗者]"));

            // 2. 杀敌仪式检测 (对应 LotMPlayer 里的计数器)
            bool killReady = modPlayer.judgmentProgress >= LotMPlayer.JUDGMENT_RITUAL_TARGET;
            string killColor = killReady ? "00FF00" : "FF0000";
            tooltips.Add(new TooltipLine(Mod, "KillReq", $"[c/{killColor}:条件2: 审判强敌 ({modPlayer.judgmentProgress}/{LotMPlayer.JUDGMENT_RITUAL_TARGET})]"));

            // 3. 环境与物品检测
            bool isDay = Main.dayTime;
            bool hasEmblem = player.HasItem(ItemID.AvengerEmblem);
            bool isClean = !player.HasBuff(BuffID.Bleeding) && !player.HasBuff(BuffID.Poisoned);

            string condColor = (isDay && hasEmblem && isClean) ? "00FF00" : "FF0000";
            string dayText = isDay ? "白天" : "需白天";
            string itemText = hasEmblem ? "徽章" : "缺复仇者徽章";
            string cleanText = isClean ? "纯净" : "有污秽Debuff";

            tooltips.Add(new TooltipLine(Mod, "CondReq", $"[c/{condColor}:条件3: {dayText} + {itemText} + {cleanText}]"));
        }

        public override bool CanUseItem(Player player)
        {
            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();
            // 只有序列4才能喝
            return modPlayer.baseSunSequence == 4;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

                if (modPlayer.baseSunSequence == 4)
                {
                    // === 1. 检查击杀仪式 ===
                    if (modPlayer.judgmentProgress < LotMPlayer.JUDGMENT_RITUAL_TARGET)
                    {
                        Main.NewText($"仪式未完成：你需要审判更多的罪恶 ({modPlayer.judgmentProgress}/{LotMPlayer.JUDGMENT_RITUAL_TARGET})。", 255, 50, 50);
                        return false; // 不消耗物品
                    }

                    // === 2. 检查环境仪式 ===
                    bool isDay = Main.dayTime;
                    bool hasEmblem = player.HasItem(ItemID.AvengerEmblem);
                    bool isClean = !player.HasBuff(BuffID.Bleeding) && !player.HasBuff(BuffID.Poisoned);

                    if (isDay && hasEmblem && isClean)
                    {
                        // 晋升成功
                        modPlayer.baseSunSequence = 3;
                        // 可选：如果不等下一帧自动同步，可以手动设置 current，让效果立刻显现
                        // modPlayer.currentSunSequence = 3; 

                        Main.NewText("你确立了自己的秩序，与世界签订了契约...", 255, 215, 0);
                        Main.NewText("晋升圣者！序列3：正义导师。", 255, 215, 0);
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item29, player.position);

                        // 圣者晋升特效：巨大的金色光柱
                        for (int i = 0; i < 100; i++)
                        {
                            Vector2 speed = new Vector2(0, -10f).RotatedByRandom(0.5f);
                            Dust d = Dust.NewDustPerfect(player.Center, DustID.GoldFlame, speed, 0, default, 4f);
                            d.noGravity = true;
                        }
                        return true;
                    }
                    else
                    {
                        // 失败提示
                        if (!isDay) Main.NewText("仪式失败：正义需要在阳光下见证。", 150, 150, 150);
                        else if (!hasEmblem) Main.NewText("仪式失败：你缺少践行正义的证明 (需持有复仇者徽章)。", 150, 150, 150);
                        else Main.NewText("仪式失败：你的身体不够纯净 (请清除流血/中毒等状态)。", 150, 150, 150);
                        return false;
                    }
                }
                else
                {
                    Main.NewText("你的灵性不足以容纳这份权柄 (需序列4)。", 150, 150, 150);
                    return false;
                }
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.FragmentSolar, 15)
                .AddIngredient(ItemID.Ectoplasm, 10)
                .AddIngredient(ItemID.LifeFruit, 5)
                .AddIngredient(ItemID.SporeSac, 1)
                .AddIngredient(ItemID.HallowedBar, 20)
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .AddTile(TileID.Bottles)
                .Register();
        }
    }
}