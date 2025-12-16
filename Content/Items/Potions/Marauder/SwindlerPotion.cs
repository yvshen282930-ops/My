using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Marauder
{
    public class SwindlerPotion : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("魔药：诈骗师");
            // Tooltip.SetDefault("序列8 诈骗师\n" +
            //                  "服用后获得质变的观察力、魅力与口才。\n" +
            //                  "获得“思维误导”与“精神干扰”能力。\n" +
            //                  "“欺诈是我的乐章。”");
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
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold: 2);
            Item.buffType = BuffID.WellFed; // 临时的饮用效果
            Item.buffTime = 600;
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();

            // 晋升逻辑：必须是序列9偷盗者(9) 才能晋升为 诈骗师(8)
            // 如果还没写偷盗者，这里假设玩家至少开启了途径
            if (modPlayer.currentMarauderSequence == 9)
            {
                modPlayer.currentMarauderSequence = 8;
                Main.NewText("你感到思维变得更加敏捷，言语中充满了魔力...", 200, 50, 200);
                // 播放音效或特效
                return true;
            }
            else if (modPlayer.currentMarauderSequence < 8)
            {
                Main.NewText("你已经掌握了更高深的欺诈技巧，无需服用。", 150, 150, 150);
            }
            else
            {
                // 如果是凡人(10)，可以直接开启途径（为了方便测试，或者要求先喝序列9）
                // 这里建议严谨一点：必须先喝序列9。如果为了测试方便，可以解开下面注释：
                // modPlayer.currentMarauderSequence = 8; 
                // return true;
                Main.NewText("失控的风险在你体内激荡... 你需要先成为偷盗者！", 255, 50, 50);
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.Worm, 1)          // 迷魂虫幼虫
                .AddIngredient(ItemID.JungleSpores, 1)  // 人面笼草 (代指)
                .AddIngredient(ItemID.Lens, 1)          // 他人眼泪
                .AddIngredient(ItemID.Sapphire, 1)      // 青金石
                .AddTile(TileID.Bottles)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}