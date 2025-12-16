using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Moon
{
    public class BeastTamerPotion : LotMItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("驯兽师魔药");
            // Tooltip.SetDefault("服用后晋升为 序列8：驯兽师\n获得动物感官，体质大幅提升");
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 30;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.maxStack = 30;
            Item.consumable = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(gold: 1);
        }

        // 使用条件：必须已经是 序列9：药师
        public override bool CanUseItem(Player player)
        {
            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();
            return modPlayer.currentMoonSequence == 9;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();
                modPlayer.currentMoonSequence = 8; // 晋升

                CombatText.NewText(player.getRect(), Color.Orange, "晋升：驯兽师", true);
                Main.NewText("你能听懂野兽的低语，力量充盈全身...", 255, 100, 100);

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, player.position); // 播放一声咆哮
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.Sapphire, 1)       // 精灵之泉结晶
                .AddIngredient(ItemID.JungleSpores, 3)   // 大王剑花
                .AddIngredient(ItemID.Daybloom, 1)       // 花
                .AddIngredient(ItemID.LesserHealingPotion, 1) // 汁液
                .AddRecipeGroup("IronBar", 1)            // 简单替代尸油或其他
                .AddTile(TileID.Bottles)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}