using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions
{
    public class SilverKnightPotion : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 30;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.UseSound = SoundID.Item3;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.maxStack = 99;
            Item.consumable = true;
            Item.rare = ItemRarityID.Cyan;
            Item.value = Item.buyPrice(platinum: 1);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<LotMPlayer>();
            if (modPlayer.currentSequence == 4)
            {
                // 【关键修复】使用 1.4.4 新版变量名
                bool frostMoonDone = NPC.downedChristmasTree && NPC.downedChristmasSantank && NPC.downedChristmasIceQueen;
                bool pumpkinMoonDone = NPC.downedHalloweenTree && NPC.downedHalloweenKing;

                string frostColor = frostMoonDone ? "00FF00" : "FF0000";
                string pumpkinColor = pumpkinMoonDone ? "00FF00" : "FF0000";

                tooltips.Add(new TooltipLine(Mod, "RitualDesc", "晋升仪式: 通关霜月与南瓜月事件，证明你的杀戮能力。"));
                tooltips.Add(new TooltipLine(Mod, "RitualStatus1", $"[c/{frostColor}:霜月 (常青树/坦克/冰女): {(frostMoonDone ? "已击杀" : "未完成")}]"));
                tooltips.Add(new TooltipLine(Mod, "RitualStatus2", $"[c/{pumpkinColor}:南瓜月 (哀木/南瓜王): {(pumpkinMoonDone ? "已击杀" : "未完成")}]"));
            }
        }

        public override bool CanUseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();
            if (modPlayer.currentSequence == 4)
            {
                // 【关键修复】使用 1.4.4 新版变量名
                bool frostMoonDone = NPC.downedChristmasTree && NPC.downedChristmasSantank && NPC.downedChristmasIceQueen;
                bool pumpkinMoonDone = NPC.downedHalloweenTree && NPC.downedHalloweenKing;

                if (!frostMoonDone || !pumpkinMoonDone)
                {
                    if (player.whoAmI == Main.myPlayer)
                        Main.NewText("祭品不足，神灵未投下注视... 需彻底征服霜月与南瓜月。", 255, 50, 50);
                    return false;
                }
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();

            if (modPlayer.currentSequence == 4)
            {
                modPlayer.currentSequence = 3;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, player.position);

                Main.NewText("这是一场献给神明的血腥祭祀！", 255, 20, 20);
                Main.NewText("晋升成功：序列3 银骑士！", 192, 192, 192);
                Main.NewText("能力：【水银化】(按C键) | 【借光隐藏】 | 【空间斩杀】", 255, 255, 255);
                return true;
            }
            else if (modPlayer.currentSequence > 4)
            {
                Main.NewText("你还未成为猎魔者。", 200, 50, 50);
                return true;
            }
            else
            {
                Main.NewText("你已是银骑士。", 200, 200, 200);
                return true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.EverscreamTrophy, 1)
                .AddIngredient(ItemID.SantaNK1Trophy, 1)
                .AddIngredient(ItemID.IceQueenTrophy, 1)
                .AddIngredient(ItemID.MourningWoodTrophy, 1)
                .AddIngredient(ItemID.PumpkingTrophy, 1)
                .AddIngredient(ItemID.SpookyWood, 50)
                .AddTile(TileID.Bottles)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}