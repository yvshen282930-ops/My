using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Moon
{
    public class ScarletScholarPotion : LotMItem
    {
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
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 10);
        }

        public override bool CanUseItem(Player player)
        {
            // 必须是 序列6：魔药教授
            return player.GetModPlayer<LotMPlayer>().baseMoonSequence == 6;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();
                modPlayer.baseMoonSequence = 5; // 晋升

                CombatText.NewText(player.getRect(), Color.Crimson, "晋升：深红学者", true);
                Main.NewText("你的眼中映照出一轮猩红的圆月...", 220, 20, 60);

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item119, player.position);
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.HallowedBar, 5)
                // 【核心修复】注意 SoulofNight 和 SoulofLight 的大小写
                .AddIngredient(ItemID.SoulofNight, 10)
                .AddIngredient(ItemID.SoulofLight, 5)
                .AddIngredient(ItemID.UnicornHorn, 1)
                .AddTile(TileID.Bottles)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}