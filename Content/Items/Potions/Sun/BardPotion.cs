using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using zhashi.Content; // 引用 LotMPlayer 所在的命名空间
using zhashi.Content.Items; // 引用 BlasphemySlate 所在的命名空间

namespace zhashi.Content.Items.Potions.Sun
{
    public class BardPotion : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 26;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.useTurn = true;
            Item.maxStack = 99;
            Item.consumable = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(silver: 10);
            Item.buffType = BuffID.WellFed;
            Item.buffTime = 3600;
        }

        public override bool CanUseItem(Player player)
        {
            // 只有当前太阳途径是序列10（凡人）时才允许尝试
            // 如果已经是歌颂者(9)或更高，直接禁饮
            return player.GetModPlayer<LotMPlayer>().baseSunSequence == 10;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                var p = player.GetModPlayer<LotMPlayer>();

                // 【核心修复】检查是否已经是其他途径的非凡者 (IsBeyonder)
                // 如果是 (比如已经是猎人)，则判定为“消化不良/排斥”，不给予晋升效果
                if (p.IsBeyonder)
                {
                    Main.NewText("你的灵性已定型，无法开启第二条途径！强行服用导致魔药失效...", 255, 50, 50);
                    // 返回 true 代表消耗掉物品 (惩罚机制)
                    return true;
                }

                // 只有通过了上面的检查，才执行晋升
                p.baseSunSequence = 9; // 修改 Base 存档变量
                p.currentSunSequence = 9; // 同步当前状态

                Main.NewText("你感觉喉咙变得灼热，心中充满了勇气！", 255, 215, 0);
                // 建议加上喝水的音效
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item3, player.position);
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Sunflower, 1)
                .AddIngredient(ItemID.Feather, 1)
                .AddIngredient(ItemID.Ale, 1)
                .AddIngredient(ItemID.Daybloom, 1)
                .AddIngredient(ItemID.Moonglow, 1)
                .AddIngredient(ModContent.ItemType<BlasphemySlate>(), 1)
                .AddTile(TileID.Bottles)
                .Register();
        }
    }
}