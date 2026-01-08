using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions
{
    public class GloryPotion : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 30; Item.useStyle = ItemUseStyleID.DrinkLiquid; Item.UseSound = SoundID.Item3;
            Item.useAnimation = 17; Item.useTime = 17; Item.useTurn = true; Item.maxStack = 99; Item.consumable = true;
            Item.rare = ItemRarityID.Red; Item.value = Item.buyPrice(platinum: 5);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Main.LocalPlayer.GetModPlayer<LotMPlayer>().baseSequence == 3)
            {
                string statusColor = NPC.downedMoonlord ? "00FF00" : "FF0000";
                tooltips.Add(new TooltipLine(Mod, "RitualDesc", "晋升仪式: 猎杀一位天使或同等强大的生物 (月亮领主)。"));
                tooltips.Add(new TooltipLine(Mod, "RitualProgress", $"[c/{statusColor}:目标状态: {(NPC.downedMoonlord ? "已陨落" : "存活")}]"));
            }
        }

        public override bool CanUseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();
            if (modPlayer.baseSequence == 3 && !NPC.downedMoonlord)
            {
                if (player.whoAmI == Main.myPlayer) Main.NewText("你的战绩不足以承载神性的光辉...", 255, 50, 50);
                return false;
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();
            if (modPlayer.baseSequence == 3)
            {
                modPlayer.baseSequence = 2;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, player.position);
                Main.NewText("黄昏的余晖在你身上凝固，你即是荣耀！", 255, 100, 0);
                Main.NewText("晋升成功：序列2 荣耀者 (天使)！", 255, 100, 0);
                Main.NewText("获得权柄：【黄昏重生】(抵挡一次死亡) | 【黄昏之笼】", 255, 255, 255);
                return true;
            }
            else if (modPlayer.baseSequence > 3) { Main.NewText("你还未成为银骑士。", 200, 50, 50); return true; }
            else { Main.NewText("你已是荣耀者。", 200, 200, 200); return true; }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                // 【关键修复】这里必须用 LunarBar，不能用 LuminiteBar
                .AddIngredient(ItemID.LunarBar, 10)
                .AddIngredient(ItemID.FragmentSolar, 10)
                .AddIngredient(ItemID.FragmentNebula, 5)
                .AddTile(TileID.LunarCraftingStation)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}