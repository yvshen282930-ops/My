using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Hunter
{
    public class ReaperPotion : ModItem
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
            Item.rare = ItemRarityID.Pink; // 粉色 (机械三王后)
            Item.value = Item.buyPrice(gold: 15);
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();

            if (modPlayer.currentHunterSequence == 6)
            {
                modPlayer.currentHunterSequence = 5;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, player.position);

                Main.NewText("你看到了万物的死线...", 255, 50, 50);
                Main.NewText("晋升成功：序列5 收割者！", 255, 50, 50);
                Main.NewText("能力：【弱点攻击】(暴击伤害提升) | 【致命攻击】(斩杀低血量) | 【屠杀】(按J键)", 255, 255, 255);
                return true;
            }
            else if (modPlayer.currentHunterSequence > 6)
            {
                Main.NewText("你还未成为阴谋家。", 200, 50, 50);
                return true;
            }
            else
            {
                Main.NewText("你早已掌握了收割的技艺。", 200, 200, 200);
                return true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.HallowedBar, 10)   // 神圣锭
                .AddIngredient(ItemID.SoulofFright, 5)   // 恐惧之魂
                .AddIngredient(ItemID.SoulofNight, 10)   // 暗影之魂
                .AddIngredient(ItemID.SharkFin, 5)       // 鲨鱼鳍 (猎杀象征)
                .AddTile(TileID.Bottles)
                .Register();
        }
    }
}