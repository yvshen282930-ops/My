using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Hunter
{
    public class WarBishopPotion : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 30; Item.useStyle = ItemUseStyleID.DrinkLiquid; Item.UseSound = SoundID.Item3;
            Item.useAnimation = 17; Item.useTime = 17; Item.useTurn = true; Item.maxStack = 99; Item.consumable = true;
            Item.rare = ItemRarityID.Cyan; // 青色 (拜月后)
            Item.value = Item.buyPrice(platinum: 2);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<LotMPlayer>();
            if (modPlayer.currentHunterSequence == 4)
            {
                string statusColor = NPC.downedAncientCultist ? "00FF00" : "FF0000";
                tooltips.Add(new TooltipLine(Mod, "RitualDesc", "晋升仪式: 击败拜月教邪教徒，开启最终的战争。"));
                tooltips.Add(new TooltipLine(Mod, "RitualProgress", $"[c/{statusColor}:目标状态: {(NPC.downedAncientCultist ? "已击杀" : "存活")}]"));
            }
        }

        public override bool CanUseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();
            if (modPlayer.currentHunterSequence == 4 && !NPC.downedAncientCultist)
            {
                if (player.whoAmI == Main.myPlayer) Main.NewText("战争的序幕还未拉开...", 255, 50, 50);
                return false;
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();
            if (modPlayer.currentHunterSequence == 4)
            {
                modPlayer.currentHunterSequence = 3;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, player.position);
                Main.NewText("你的意志连接了整个战场！", 255, 69, 0);
                Main.NewText("晋升成功：序列3 战争主教 (圣者)！", 255, 69, 0);
                Main.NewText("能力：【心灵网络】(属性共享) | 【战争兵器】(哨兵强化) | 【L键集众强化】", 255, 255, 255);
                return true;
            }
            else if (modPlayer.currentHunterSequence > 4) { Main.NewText("你还未成为铁血骑士。", 200, 50, 50); return true; }
            else { Main.NewText("你已是战争主教。", 200, 200, 200); return true; }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.SpectreBar, 10)   // 幽灵锭
                .AddIngredient(ItemID.ShroomiteBar, 10) // 蘑菇矿
                .AddIngredient(ItemID.LihzahrdPowerCell, 1) // 电池 (代表战争兵器)
                .AddTile(TileID.LunarCraftingStation)   // 远古操纵机
                .Register();
        }
    }
}