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
                string statusColor = (modPlayer.weatherRitualCount >= 5) ? "00FF00" : "FF0000";
                tooltips.Add(new TooltipLine(Mod, "RitualDesc", "晋升仪式: 使用[气象符文]，在10秒内强行改变5次天气。"));
                tooltips.Add(new TooltipLine(Mod, "RitualProgress", $"[c/{statusColor}:仪式状态: {(modPlayer.weatherRitualCount >= 5 ? "已完成" : "未完成")}]"));
            }
        }

        public override bool CanUseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();
            if (modPlayer.currentHunterSequence == 3 && modPlayer.weatherRitualCount < 5)
            {
                if (player.whoAmI == Main.myPlayer) Main.NewText("你还未展现出对天气的绝对掌控...", 255, 50, 50);
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
                return true;
            }
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.FragmentSolar, 10) // 日耀
                .AddIngredient(ItemID.FragmentVortex, 10) // 星旋 (风暴)
                .AddIngredient(ItemID.FragmentNebula, 10) // 星云
                .AddIngredient(ItemID.FragmentStardust, 10) // 星尘
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}