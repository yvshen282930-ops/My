using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Hunter
{
    public class ProvokerPotion : ModItem
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
            Item.rare = ItemRarityID.Blue; // 蓝色稀有度
            Item.value = Item.buyPrice(gold: 1);
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<LotMPlayer>();

            // 必须先成为 序列9 猎人
            if (modPlayer.currentHunterSequence == 9)
            {
                modPlayer.currentHunterSequence = 8;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, player.position);

                Main.NewText("你感觉喉咙里像吞了一块火炭，想要大声嘲笑这个世界...", 255, 150, 50);
                Main.NewText("晋升成功：序列8 挑衅者！", 255, 150, 50);
                Main.NewText("获得能力：【挑衅】(大幅吸引仇恨) | 【格斗体魄】", 255, 255, 255);
                return true;
            }
            else if (modPlayer.currentHunterSequence > 9)
            {
                Main.NewText("你还未成为猎人，无法消化这份非凡特性。", 200, 50, 50);
                return true;
            }
            else
            {
                Main.NewText("你已经是更高序列的强者了。", 200, 200, 200);
                return true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.Ale, 1)         // 蒸馏酒 -> 麦芽酒
                .AddIngredient(ItemID.Vine, 2)        // 葡萄藤 -> 藤蔓
                .AddIngredient(ItemID.Obsidian, 5)    // 深黑石头 -> 黑曜石
                .AddIngredient(ItemID.Waterleaf, 2)   // 水蕨草 -> 水叶草
                .AddTile(TileID.Bottles)
                .AddIngredient(ModContent.ItemType<Items.BlasphemySlate>(), 1)
                .Register();
        }
    }
}