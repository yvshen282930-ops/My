using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Hunter
{
    public class IronBloodedKnightPotion : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 30; Item.useStyle = ItemUseStyleID.DrinkLiquid; Item.UseSound = SoundID.Item3;
            Item.useAnimation = 17; Item.useTime = 17; Item.useTurn = true; Item.maxStack = 99; Item.consumable = true;
            Item.rare = ItemRarityID.Yellow; Item.value = Item.buyPrice(gold: 50);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<LotMPlayer>();
            if (modPlayer.currentHunterSequence == 5)
            {
                string statusColor = (modPlayer.ironBloodRitualProgress >= LotMPlayer.IRON_BLOOD_RITUAL_TARGET) ? "00FF00" : "FF0000";
                string progressText = $"[c/{statusColor}:征服进度: {modPlayer.ironBloodRitualProgress} / {LotMPlayer.IRON_BLOOD_RITUAL_TARGET}]";

                // 【修改】说明更新
                tooltips.Add(new TooltipLine(Mod, "RitualDesc", "晋升仪式: 在至少有5名随从(召唤物)的情况下，无死亡累计击杀100个敌人。"));
                tooltips.Add(new TooltipLine(Mod, "RitualProgress", progressText));
            }
        }

        public override bool CanUseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();
            if (modPlayer.currentHunterSequence == 5)
            {
                if (modPlayer.ironBloodRitualProgress < LotMPlayer.IRON_BLOOD_RITUAL_TARGET)
                {
                    if (player.whoAmI == Main.myPlayer) Main.NewText("你的军队还未经历足够的铁与血...", 255, 50, 50);
                    return false;
                }
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();
            if (modPlayer.currentHunterSequence == 5)
            {
                modPlayer.currentHunterSequence = 4;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, player.position);
                Main.NewText("火焰与钢铁在你体内重铸！", 255, 69, 0);
                Main.NewText("晋升成功：序列4 铁血骑士！", 255, 69, 0);
                Main.NewText("能力：【钢铁之躯】 | 【火焰化】(K) | 【集众】(L)", 255, 255, 255);
                return true;
            }
            else if (modPlayer.currentHunterSequence > 5) { Main.NewText("你还未成为收割者。", 200, 50, 50); return true; }
            else { Main.NewText("你已是铁血骑士。", 200, 200, 200); return true; }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.BeetleHusk, 10)
                .AddIngredient(ItemID.HellstoneBar, 10)
                .AddIngredient(ItemID.SoulofMight, 5)
                .AddIngredient(ItemID.SoulofSight, 5)
                .AddTile(TileID.Bottles)
                .Register();
        }
    }
}