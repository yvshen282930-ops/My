using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content; // 引用玩家数据类

namespace zhashi.Content.Items.Potions
{
    public class WarriorPotion : ModItem
    {
        // 设置物品的基本属性
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 26;

            // 【关键修改 1】动作设置为喝液体
            Item.useStyle = ItemUseStyleID.DrinkLiquid;

            // 【关键修改 2】增加喝水的声音 (咕嘟咕嘟)
            Item.UseSound = SoundID.Item3;


            Item.useAnimation = 17; // 动画播放时间
            Item.useTime = 17;      // 实际使用时间
            Item.useTurn = true;
            Item.maxStack = 99;
            Item.consumable = true; // 消耗品
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(silver: 50);
        }

        // 当玩家使用物品时发生什么
        public override bool? UseItem(Player player)
        {
            // 获取我们写的 zhashi 玩家数据
            var modPlayer = player.GetModPlayer<LotMPlayer>();

            // 只有普通人(序列10)才能晋升序列9
            if (modPlayer.currentSequence == 10)
            {
                modPlayer.currentSequence = 9;

                // 在左下角显示提示
                Main.NewText("你服用了战士魔药，感觉身体充满了力量！", 255, 100, 100);
                return true;
            }
            else
            {
                // 如果已经是序列9或更高，提示无效
                Main.NewText("你已经是超凡者了，这瓶低阶魔药对你无效。", 200, 200, 200);
                return true;
            }
        }

        // 制作配方
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1) // 1个水瓶
                .AddIngredient(ItemID.Daybloom, 1)     // 1个太阳花
                .AddRecipeGroup(RecipeGroupID.IronBar, 3) // 3个铁锭或铅锭
                .AddTile(TileID.Bottles)               // 在放置的瓶子边制作
                .Register();
        }
    }
}