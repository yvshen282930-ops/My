using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Hunter
{
    public class WeatherWarlockPotion : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 30; Item.useStyle = ItemUseStyleID.DrinkLiquid; Item.UseSound = SoundID.Item3;
            Item.useAnimation = 17; Item.useTime = 17; Item.useTurn = true; Item.maxStack = 99; Item.consumable = true;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.buyPrice(platinum: 5);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<LotMPlayer>();
            if (modPlayer.currentHunterSequence == 3)
            {
                // 【修改1】判定标准改为 10次 (与符文一致)
                // 【修改2】直接读取 weatherRitualComplete 标记，而不是不稳定的 count
                string statusColor = modPlayer.weatherRitualComplete ? "00FF00" : "FF0000";

                tooltips.Add(new TooltipLine(Mod, "RitualDesc", "晋升仪式: 使用[气象符文]，连续触发 10 次天气共鸣。"));
                tooltips.Add(new TooltipLine(Mod, "RitualProgress", $"[c/{statusColor}:仪式状态: {(modPlayer.weatherRitualComplete ? "已完成" : "未完成")}]"));

                // 可选：显示当前进度 (如果未完成)
                if (!modPlayer.weatherRitualComplete && modPlayer.weatherRitualCount > 0)
                {
                    tooltips.Add(new TooltipLine(Mod, "RitualCount", $"当前共鸣: {modPlayer.weatherRitualCount}/10"));
                }
            }
        }

        public override bool CanUseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();

            // 【修改3】核心修复：检测永久标记 weatherRitualComplete
            // 只要符文触发过一次成功（达到10次），这个标记就为 true，永远不会被清零
            if (modPlayer.currentHunterSequence == 3 && !modPlayer.weatherRitualComplete)
            {
                if (player.whoAmI == Main.myPlayer) Main.NewText("你还未展现出对天气的绝对掌控... (需连续使用符文10次)", 255, 50, 50);
                return false;
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();
            if (modPlayer.currentHunterSequence == 3)
            {
                modPlayer.currentHunterSequence = 2;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, player.position);
                Main.NewText("风暴、雷电、迷雾听从你的号令！", 0, 255, 255);
                Main.NewText("晋升成功：序列2 天气术士 (天使)！", 0, 255, 255);

                // 晋升后重置仪式状态，方便下次（虽然没有下次了）
                modPlayer.weatherRitualComplete = false;
                modPlayer.weatherRitualCount = 0;

                return true;
            }
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.FragmentSolar, 10)
                .AddIngredient(ItemID.FragmentVortex, 10)
                .AddIngredient(ItemID.FragmentNebula, 10)
                .AddIngredient(ItemID.FragmentStardust, 10)
                .AddTile(TileID.LunarCraftingStation)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}