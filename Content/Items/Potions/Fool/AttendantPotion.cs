using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;
using zhashi.Content.Buffs;
using System.Collections.Generic;

namespace zhashi.Content.Items.Potions.Fool
{
    public class AttendantPotion : LotMItem
    {
        public override string Pathway => "Fool";
        public override int RequiredSequence => 2;

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
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.sellPrice(platinum: 50);
        }

        // 【核心修复】不再数身边的NPC，而是读取 LotMPlayer 中保存的进度
        public override bool CanUseItem(Player player)
        {
            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

            // 只要仪式完成标记为 true，或者进度计数达到 10，就允许使用
            bool conditionMet = modPlayer.attendantRitualComplete || modPlayer.attendantRitualProgress >= 10;

            if (!conditionMet)
            {
                return false;
            }

            return base.CanUseItem(player);
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();
                modPlayer.currentFoolSequence = 1;

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, player.position);
                Main.NewText("城镇在你的意志下起舞，诡秘的帷幕已然拉开...", 148, 0, 211);
                Main.NewText("晋升成功！序列1：诡秘侍者！(天使之王)", 255, 0, 255);
            }
            return true;
        }

        // 【UI修复】显示存档中的真实进度
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(tooltips);

            Player player = Main.LocalPlayer;
            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

            // 读取保存的进度
            int count = modPlayer.attendantRitualProgress;
            bool complete = modPlayer.attendantRitualComplete || count >= 10;

            if (count > 10) count = 10; // 显示上限锁定为10

            string color = complete ? "00FF00" : "FF0000"; // 完成变绿，未完成变红
            string status = complete ? " (已完成)" : "";

            tooltips.Add(new TooltipLine(Mod, "Ritual", $"[c/{color}:仪式要求：转化10个城镇NPC ({count}/10){status}]"));
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.LunarBar, 20)       // 诡秘侍者特性
                .AddIngredient(ItemID.Ectoplasm, 5)       // 灵气
                .AddIngredient(ItemID.SoulofFlight, 5)    // 飞翔之魂
                .AddIngredient(ItemID.SoulofLight, 5)     // 光明之魂
                .AddIngredient(ItemID.SoulofNight, 5)     // 暗影之魂
                .AddIngredient(ItemID.UnicornHorn, 1)     // 独角兽角
                .AddIngredient(ItemID.PixieDust, 10)      // 精灵尘
                .AddIngredient(ItemID.CrystalShard, 5)    // 水晶碎块
                .AddIngredient(ItemID.CursedFlame, 5)     // 咒火
                .AddIngredient(ItemID.Ichor, 5)           // 灵液
                .AddTile(TileID.LunarCraftingStation)     // 远古操纵机
                .Register();
        }
    }
}