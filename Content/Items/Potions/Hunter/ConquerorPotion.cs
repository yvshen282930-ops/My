using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Hunter
{
    public class ConquerorPotion : ModItem
    {
        public override void SetStaticDefaults()
        {
            // 名字和描述在 HJSON 中
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
            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(gold: 50);
        }

        // 【核心修复】在这里添加动态文本，告诉玩家条件是否满足
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

            // 1. 序列检测
            string seqColor = (modPlayer.currentHunterSequence == 2) ? "00FF00" : "FF0000";
            string seqText = (modPlayer.currentHunterSequence == 2) ? "已满足" : "未满足 (需序列2)";
            tooltips.Add(new TooltipLine(Mod, "SeqReq", $"[c/{seqColor}:条件1: {seqText}]"));

            // 2. 仪式检测
            string ritualColor = modPlayer.conquerorRitualComplete ? "00FF00" : "FF0000";
            string ritualText = modPlayer.conquerorRitualComplete ? "仪式已完成" : "仪式未完成";
            tooltips.Add(new TooltipLine(Mod, "RitualReq", $"[c/{ritualColor}:条件2: {ritualText}]"));

            if (!modPlayer.conquerorRitualComplete)
            {
                tooltips.Add(new TooltipLine(Mod, "RitualHint", "仪式说明: 使用[征服者特性]让世界静默，并确认全图无敌手。"));
            }
        }

        public override bool CanUseItem(Player player)
        {
            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();
            if (modPlayer.currentHunterSequence <= 1) return false; // 已经是序列1了

            // 只有同时满足两个条件才能喝
            if (modPlayer.currentHunterSequence == 2 && modPlayer.conquerorRitualComplete)
            {
                return true;
            }
            return false;
        }

        public override bool? UseItem(Player player)
        {
            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();
            modPlayer.currentHunterSequence = 1;

            Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, player.position);

            if (player.whoAmI == Main.myPlayer)
            {
                Main.NewText("晋升成功！序列1：征服者！", 255, 69, 0);
                Main.NewText("战火已熄，生态恢复...", 100, 255, 100);
            }

            // 恢复刷怪
            ConquerorSpawnSystem.StopSpawning = false;
            return true;
        }
    }
}