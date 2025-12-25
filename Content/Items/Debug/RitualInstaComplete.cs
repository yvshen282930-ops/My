using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using zhashi.Content; // 引用玩家数据

namespace zhashi.Content.Items.Debug
{
    public class RitualInstaComplete : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.Stopwatch;

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("仪式一键完成怀表 (Dev)");
            // Tooltip.SetDefault("【开发者调试物品】\n" +
            //                  "右键使用：\n" +
            //                  "1. 强制拉满所有途径的晋升仪式进度\n" +
            //                  "2. 重置所有技能的冷却时间 (月亮/太阳/愚者)\n" +
            //                  "3. 补满灵性");
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.rare = ItemRarityID.Red;
            Item.consumable = false;
            Item.value = 0;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                var modPlayer = player.GetModPlayer<LotMPlayer>();
                bool anyChange = false;

                // ==========================================
                // 1. 仪式进度拉满 (针对有进度条的途径)
                // ==========================================

                // [巨人]
                if (modPlayer.guardianRitualProgress < LotMPlayer.GUARDIAN_RITUAL_TARGET) { modPlayer.guardianRitualProgress = LotMPlayer.GUARDIAN_RITUAL_TARGET; anyChange = true; }

                // [猎人]
                if (modPlayer.demonHunterRitualProgress < LotMPlayer.DEMON_HUNTER_RITUAL_TARGET) { modPlayer.demonHunterRitualProgress = LotMPlayer.DEMON_HUNTER_RITUAL_TARGET; anyChange = true; }
                if (modPlayer.ironBloodRitualProgress < LotMPlayer.IRON_BLOOD_RITUAL_TARGET) { modPlayer.ironBloodRitualProgress = LotMPlayer.IRON_BLOOD_RITUAL_TARGET; anyChange = true; }
                if (!modPlayer.weatherRitualComplete) { modPlayer.weatherRitualComplete = true; anyChange = true; }
                if (!modPlayer.conquerorRitualComplete) { modPlayer.conquerorRitualComplete = true; anyChange = true; }

                // [愚者]
                if (modPlayer.attendantRitualProgress < LotMPlayer.ATTENDANT_RITUAL_TARGET) { modPlayer.attendantRitualProgress = LotMPlayer.ATTENDANT_RITUAL_TARGET; modPlayer.attendantRitualComplete = true; anyChange = true; }

                // [错误]
                if (modPlayer.parasiteRitualProgress < LotMPlayer.PARASITE_RITUAL_TARGET) { modPlayer.parasiteRitualProgress = LotMPlayer.PARASITE_RITUAL_TARGET; anyChange = true; }
                if (modPlayer.mentorRitualProgress < LotMPlayer.MENTOR_RITUAL_TARGET) { modPlayer.mentorRitualProgress = LotMPlayer.MENTOR_RITUAL_TARGET; anyChange = true; }
                if (modPlayer.trojanRitualTimer < LotMPlayer.TROJAN_RITUAL_TARGET) { modPlayer.trojanRitualTimer = LotMPlayer.TROJAN_RITUAL_TARGET; anyChange = true; }
                if (modPlayer.wormRitualTimer < LotMPlayer.WORM_RITUAL_TARGET) { modPlayer.wormRitualTimer = LotMPlayer.WORM_RITUAL_TARGET; anyChange = true; }

                // ==========================================
                // 2. 冷却时间重置 (针对 月亮/太阳/愚者)
                // ==========================================
                // 月亮途径 (Moon) - 大招流
                modPlayer.paperFigurineCooldown = 0;   // 序列2: 纸人替身 (5分钟CD)
                modPlayer.darknessGazeCooldown = 0;    // 序列4: 黑暗凝视
                modPlayer.summonGateCooldown = 0;      // 序列3: 召唤之门
                modPlayer.purifyCooldown = 0;          // 序列2: 净化大地
                modPlayer.elixirCooldown = 0;          // 序列6: 生命灵液
                modPlayer.abyssShackleCooldown = 0;    // 序列7: 深渊枷锁

                // 太阳途径 (Sun) - 技能流
                modPlayer.sunRadianceCooldown = 0;     // 序列8: 日照
                modPlayer.holyLightCooldown = 0;       // 序列7: 召唤圣光
                modPlayer.holyOathCooldown = 0;        // 序列7: 神圣誓约
                modPlayer.fireOceanCooldown = 0;       // 序列7: 光明之火

                // 愚者/错误/猎人 CD
                modPlayer.twilightResurrectionCooldown = 0; // 复活CD
                modPlayer.wormificationCooldown = 0;        // 半虫化CD
                modPlayer.miracleCooldown = 0;              // 奇迹CD
                modPlayer.divinationCooldown = 0;           // 占卜CD
                modPlayer.damageTransferCooldown = 0;       // 伤害转移CD
                modPlayer.glacierCooldown = 0;              // 冰河世纪CD

                // ==========================================
                // 3. 状态补满
                // ==========================================
                modPlayer.spiritualityCurrent = modPlayer.spiritualityMax; // 灵性回满
                modPlayer.borrowUsesDaily = 0; // 重置昨日重现次数 (愚者)

                // ==========================================
                // 4. 反馈
                // ==========================================
                Main.NewText("★ 开发者指令执行完毕 ★", 0, 255, 255);
                if (anyChange) Main.NewText("- 晋升仪式进度：[已拉满]", 255, 215, 0);
                Main.NewText("- 技能冷却时间：[已重置]", 100, 255, 100);
                Main.NewText("- 灵性状态：[已回满]", 100, 100, 255);

                SoundEngine.PlaySound(SoundID.Item4, player.position);
                for (int i = 0; i < 30; i++) Dust.NewDust(player.position, player.width, player.height, DustID.Electric, 0, 0, 0, default, 1.5f);
            }
            return true;
        }

        public override void AddRecipes() { }
    }
}