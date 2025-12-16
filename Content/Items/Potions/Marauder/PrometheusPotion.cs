using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Marauder
{
    public class PrometheusPotion : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("魔药：盗火人");
            // Tooltip.SetDefault("序列6 盗火人\n" +
            //                  "窃取是你的权柄，无论是物品、能力还是命运。\n" +
            //                  "获得“窃取”（攻击概率获得掉落物）与“窃取能力”（吸取属性）。\n" +
            //                  "大幅提升体质与精神抗性。\n" +
            //                  "“这是普罗米修斯的火种。”");
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 26;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.maxStack = 30;
            Item.consumable = true;
            Item.rare = ItemRarityID.Pink; // 序列6 稀有度提升
            Item.value = Item.sellPrice(gold: 5);
            Item.buffType = BuffID.OnFire; // 服用时的副作用：仿佛喝下火焰
            Item.buffTime = 300;
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();

            if (modPlayer.currentMarauderSequence == 7)
            {
                modPlayer.currentMarauderSequence = 6;
                Main.NewText("你感觉到体内的灵性燃烧起来，双手仿佛能探入虚空窃取万物...", 255, 100, 0);
                return true;
            }
            else if (modPlayer.currentMarauderSequence < 6)
            {
                Main.NewText("你已经掌握了盗火的奥秘，无需再次服用。", 150, 150, 150);
                return false;
            }
            else
            {
                Main.NewText("你无法承受这份火焰，需要先成为解密学者！", 255, 50, 50);
                return true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.CrystalShard, 1)    // 水晶线虫
                .AddIngredient(ItemID.Ale, 1)             // 美酒
                .AddIngredient(ItemID.Topaz, 10)           // 黄水晶
                .AddIngredient(ItemID.LivingFireBlock, 1) // 火种 (如果想简单点可以换成 ItemID.Torch)
                .AddTile(TileID.Bottles)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}