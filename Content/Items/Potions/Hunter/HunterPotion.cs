using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Hunter
{
    public class HunterPotion : ModItem
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
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(silver: 50);
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();

            // 互斥检查：如果已经走了巨人途径 (currentSequence < 10)，则不能喝
            if (modPlayer.currentSequence < 10)
            {
                Main.NewText("你体内已经有了巨人途径的非凡特性，强行服用会导致失控！", 255, 50, 50);
                return true; // 消耗掉作为惩罚
            }

            // 正常晋升
            if (modPlayer.currentHunterSequence == 10)
            {
                modPlayer.currentHunterSequence = 9;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, player.position);
                Main.NewText("你服用了猎人魔药，感官变得敏锐...", 255, 100, 100);
                Main.NewText("晋升成功：序列9 猎人！", 255, 100, 100);
                return true;
            }
            else
            {
                Main.NewText("你已经是猎人了。", 200, 200, 200);
                return true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.Ale, 1)         // 红葡萄酒 -> 麦芽酒
                .AddIngredient(ItemID.Daybloom, 1)    // 罗勒 -> 太阳花
                .AddIngredient(ItemID.Gel, 5)         // 辅助材料
                .AddTile(TileID.Bottles)
                .Register();
        }
    }
}