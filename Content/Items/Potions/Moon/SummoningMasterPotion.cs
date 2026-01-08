using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using zhashi.Content;
using zhashi.Content.Items.Materials; // 引用刚才的材料

namespace zhashi.Content.Items.Potions.Moon
{
    public class SummoningMasterPotion : LotMItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 34;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.maxStack = 30;
            Item.consumable = true;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(gold: 50);
        }

        public override bool CanUseItem(Player player)
        {
            // 必须是 序列4：巫王
            return player.GetModPlayer<LotMPlayer>().baseMoonSequence == 4;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();
                modPlayer.baseMoonSequence = 3; // 晋升

                CombatText.NewText(player.getRect(), Color.MediumPurple, "晋升：召唤大师", true);
                Main.NewText("你听到了灵界万千生物的呼唤，你是它们的主宰...", 200, 100, 255);

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item119, player.position);
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ModContent.ItemType<TrueNameInscription>(), 1) // 真名拓本
                .AddIngredient(ItemID.Ectoplasm, 10)    // 灵气
                .AddIngredient(ItemID.SpectreBar, 5)    // 幽灵锭 (灵界物质)
                .AddTile(TileID.Bottles)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}