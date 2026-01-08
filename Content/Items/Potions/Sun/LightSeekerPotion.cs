using Microsoft.Xna.Framework;
using System.Collections.Generic;
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

        // 【新增】动态提示，让玩家一眼看清缺什么条件
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

            // 1. 序列检测 (这是防止其他途径引用的核心)
            bool seqReady = modPlayer.baseSunSequence == 3;
            string seqColor = seqReady ? "00FF00" : "FF0000";
            // 如果不是序列3 (比如是序列10凡人，或者序列3猎人)，都会显示红色未满足
            tooltips.Add(new TooltipLine(Mod, "SeqReq", $"[c/{seqColor}:条件1: 需序列3 正义导师]"));

            // 2. 仪式检测
            bool isDay = Main.dayTime;
            bool hasArtifact = player.HasItem(ItemID.DayBreak) || player.HasItem(ItemID.SolarEruption);
            bool isHealthy = player.statLife >= player.statLifeMax2;

            string condColor = (isDay && hasArtifact && isHealthy) ? "00FF00" : "FF0000";

            string dayText = isDay ? "正午" : "需白天";
            string itemText = hasArtifact ? "圣物" : "缺日耀圣物"; // 破晓或日耀喷发剑
            string hpText = isHealthy ? "状态完好" : "需满血";

            tooltips.Add(new TooltipLine(Mod, "CondReq", $"[c/{condColor}:条件2: {dayText} + {itemText} + {hpText}]"));
        }

        // 【新增】预判拦截
        public override bool CanUseItem(Player player)
        {
            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

            // 严格限制：只有太阳途径序列 3 才能喝
            // 其他途径（如猎人）的 baseSunSequence 是 10，这里会返回 false，无法使用
            return modPlayer.baseSunSequence == 3;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

                // 双重保险检查
                if (modPlayer.baseSunSequence == 3)
                {
                    // === 仪式条件检查 ===
                    bool isDay = Main.dayTime;
                    // 只要背包里有就行，不用非得拿在手上
                    bool hasArtifact = player.HasItem(ItemID.DayBreak) || player.HasItem(ItemID.SolarEruption);
                    bool isHealthy = player.statLife >= player.statLifeMax2;

                    if (isDay && hasArtifact && isHealthy)
                    {
                        // 晋升逻辑：修改 Base 变量
                        modPlayer.baseSunSequence = 2;
                        // 为了让特效即时同步，可以手动设置一下 current (虽然下一帧会自动重置)
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
                        // 失败提示
                        if (!isDay) Main.NewText("仪式失败：必须在烈日下进行。", 150, 150, 150);
                        else if (!hasArtifact) Main.NewText("仪式失败：背包中需携带天使级太阳圣物(破晓/日耀喷发)作为道标。", 150, 150, 150);
                        else Main.NewText("仪式失败：你需要保持最完美的状态(满血)以维持意识。", 150, 150, 150);
                        return false; // 不消耗物品
                    }
                }
                else
                {
                    // 这里其实很少会触发，因为 CanUseItem 已经拦截了
                    Main.NewText("你的灵性不足以容纳这份权柄 (需序列3)。", 150, 150, 150);
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
                .AddIngredient(ItemID.Book, 1) // 或者是 ModContent.ItemType<Materials.ConquerorCharacteristic>()
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .AddIngredient(ItemID.BottledWater, 1)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}