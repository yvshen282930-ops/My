using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Hunter
{
    public class ConspiratorPotion : ModItem
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
            Item.rare = ItemRarityID.LightRed; // 浅红色 (肉后初期)
            Item.value = Item.buyPrice(gold: 10);
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();

            if (modPlayer.baseHunterSequence == 7)
            {
                modPlayer.baseHunterSequence = 6;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, player.position);

                Main.NewText("你的思维变得冰冷而清晰...", 255, 100, 0);
                Main.NewText("晋升成功：序列6 阴谋家！", 255, 100, 0);
                Main.NewText("能力：【火焰闪现】(按V键) | 【洞察】(暴击提升)", 255, 255, 255);
                return true;
            }
            else if (modPlayer.baseHunterSequence > 7)
            {
                Main.NewText("你还未掌握纵火的艺术。", 200, 50, 50);
                return true;
            }
            else
            {
                Main.NewText("你已是阴谋家。", 200, 200, 200);
                return true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.SpiderFang, 5)      // 蜘蛛材料
                .AddIngredient(ItemID.AntlionMandible, 5) // 狮子类材料
                .AddIngredient(ItemID.Amber, 3)           // 琥珀
                .AddIngredient(ItemID.Acorn, 5)           // 橡果
                .AddTile(TileID.Bottles)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}