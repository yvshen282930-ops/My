using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions
{
    public class DemonHunterPotion : ModItem
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
            Item.rare = ItemRarityID.Yellow; // 黄色稀有度 (花后/石巨人)
            Item.value = Item.buyPrice(gold: 50);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<LotMPlayer>();
            if (modPlayer.baseSequence == 5)
            {
                string statusColor = (modPlayer.demonHunterRitualProgress >= LotMPlayer.DEMON_HUNTER_RITUAL_TARGET) ? "00FF00" : "FF0000";
                string progressText = $"[c/{statusColor}:仪式进度: {modPlayer.demonHunterRitualProgress} / {LotMPlayer.DEMON_HUNTER_RITUAL_TARGET}]";

                tooltips.Add(new TooltipLine(Mod, "RitualDesc", "晋升仪式: 独自猎杀 10 只红魔鬼(Red Devil)。"));
                tooltips.Add(new TooltipLine(Mod, "RitualProgress", progressText));
            }
        }

        public override bool CanUseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();
            if (modPlayer.baseSequence == 5)
            {
                if (modPlayer.demonHunterRitualProgress < LotMPlayer.DEMON_HUNTER_RITUAL_TARGET)
                {
                    if (player.whoAmI == Main.myPlayer)
                    {
                        Main.NewText($"你还未证明你是恶魔的克星... (进度: {modPlayer.demonHunterRitualProgress}/{LotMPlayer.DEMON_HUNTER_RITUAL_TARGET})", 255, 50, 50);
                    }
                    return false;
                }
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();

            if (modPlayer.baseSequence == 5)
            {
                modPlayer.baseSequence = 4;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, player.position);

                Main.NewText("你的双眼看穿了万物的弱点，你已不再是凡人...", 255, 0, 255); // 紫色文字
                Main.NewText("晋升成功：序列4 猎魔者 (半神)！", 255, 0, 255);
                Main.NewText("获得能力：【猎魔之眼】(永久显示敌人，暴击大幅提升)", 255, 255, 255);
                Main.NewText("获得能力：【弱点看破】(攻击削弱敌人防御)", 255, 255, 255);
                return true;
            }
            else if (modPlayer.baseSequence > 5)
            {
                Main.NewText("你还未成为守护者，无法承受半神之力。", 200, 50, 50);
                return true;
            }
            else
            {
                Main.NewText("你已经是半神了。", 200, 200, 200);
                return true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.BeetleHusk, 5) // 甲虫壳 (石巨人掉落，代表坚硬材料)
                .AddIngredient(ItemID.Ectoplasm, 10) // 灵气 (地牢幽灵掉落，代表灵性)
                .AddIngredient(ItemID.SoulofNight, 15) // 暗影之魂
                .AddTile(TileID.Bottles)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}