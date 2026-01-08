using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Marauder
{
    public class CryptologistPotion : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("魔药：解密学者");
            // Tooltip.SetDefault("序列7 解密学者\n" +
            //                  "赋予使用者神秘学知识与非凡的直觉。\n" +
            //                  "你可以通过观察蛛丝马迹，还原真相，洞察敌人的弱点。\n" +
            //                  "“一切谜题都必有答案。”");
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
            Item.rare = ItemRarityID.LightRed; // 序列7开始变得稀有
            Item.value = Item.sellPrice(gold: 3);
            Item.buffType = BuffID.Darkness; // 服用时的副作用：脑袋刺痛/视野剥离
            Item.buffTime = 120;
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();

            // 晋升逻辑：必须是序列8 诈骗师 才能晋升
            if (modPlayer.baseMarauderSequence == 8)
            {
                modPlayer.baseMarauderSequence = 7;
                Main.NewText("你的大脑仿佛被剥离了皮层，无数纷乱的线条在眼中重组为真相...", 200, 50, 200);
                return true;
            }
            else if (modPlayer.baseMarauderSequence < 7)
            {
                Main.NewText("你已经掌握了真理的钥匙，无需再次服用。", 150, 150, 150);
                return false;
            }
            else
            {
                Main.NewText("你无法解析其中的奥秘，需要先成为诈骗师！", 255, 50, 50);
                return true; // 消耗掉作为惩罚
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.AntlionMandible, 5) // 狮身人材料
                .AddIngredient(ItemID.EnchantedNightcrawler, 1) // 迷魂虫成虫
                .AddIngredient(ItemID.JungleRose, 1)      // 野玫瑰
                .AddIngredient(ItemID.Diamond, 1)         // 月长石
                .AddIngredient(ItemID.Book, 1)            // 密码
                .AddTile(TileID.Bottles)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}