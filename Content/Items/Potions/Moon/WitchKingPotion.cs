using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using zhashi.Content;

namespace zhashi.Content.Items.Potions.Moon
{
    public class WitchKingPotion : LotMItem
    {
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 34;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.maxStack = 30;
            Item.consumable = true;
            Item.rare = ItemRarityID.Yellow; // 稀有度提升 (半神级)
            Item.value = Item.sellPrice(gold: 20);
        }

        // 检查使用条件
        public override bool CanUseItem(Player player)
        {
            LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();

            // 1. 必须是序列5：深红学者
            if (modPlayer.currentMoonSequence != 5) return false;

            // 2. 【核心仪式】必须是晚上 (Main.dayTime == false) 且 满月 (Main.moonPhase == 0)
            if (Main.dayTime)
            {
                return false; // 白天不能用
            }

            if (Main.moonPhase != 0) // 0 代表满月
            {
                return false;
            }

            return true;
        }

        // 当试图使用但条件不满足时 (给玩家提示)
        public override bool ConsumeItem(Player player)
        {
            // CanUseItem 返回 true 才会执行到这里，所以这里不需要写逻辑
            // 我们用 HoldItem 或者 OnMissingRequirement 来提示比较麻烦
            // 最简单的是玩家点了没反应，他自己会去看描述
            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                LotMPlayer modPlayer = player.GetModPlayer<LotMPlayer>();
                modPlayer.currentMoonSequence = 4; // 晋升：巫王

                CombatText.NewText(player.getRect(), Color.DarkViolet, "晋升：巫王！", true);
                Main.NewText("你感受到了来自月亮最深处的黑暗...", 180, 0, 255);
                Main.NewText("仪式成功：满月见证了你的加冕。", 255, 255, 255);

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item119, player.position);
            }
            return true;
        }

        public override void AddRecipes()
        {
            // 配方：半神级材料
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater, 1)
                .AddIngredient(ItemID.Ectoplasm, 10)     // 灵气 (花后材料)
                .AddIngredient(ItemID.DarkShard, 3)      // 暗黑碎片 (黑暗生物)
                .AddIngredient(ItemID.SoulofNight, 15)   // 浓郁黑暗
                .AddIngredient(ItemID.MoonStone, 1)      // 月亮石 (核心)
                .AddTile(TileID.Bottles)
                .Register();
        }
    }
}